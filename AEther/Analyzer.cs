using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Dynamic;
using System.Xml;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace AEther
{
    public class Analyzer
    {

        public event EventHandler<SampleEvent<double>>? OnSamplesAvailable;
        public event EventHandler? OnStopped;

        public readonly SampleFormat Format;
        public readonly Domain Domain;

        readonly int BatchSize;
        readonly byte[] Batch;
        readonly DFTProcessor[] DFT;
        readonly Splitter[] Splitter;
        readonly Stopwatch SplitterTimer = Stopwatch.StartNew();
        readonly TimeSpan SplitterInterval;
        readonly ConcurrentQueue<SampleEvent<byte>> InputQueue = new();
        readonly Pipe SamplePipe = new();
        readonly CancellationTokenSource Cancel = new();
        readonly TransformBlock<SampleEvent<double>, SampleEvent<double>> DFTBlock;
        readonly TransformBlock<SampleEvent<double>, SampleEvent<double>> SplitterBlock;
        readonly ActionBlock<SampleEvent<double>> OutputBlock;
        readonly Task InputTask = Task.CompletedTask;
        readonly Task BatcherTask = Task.CompletedTask;

        public Analyzer(SampleFormat format, AnalyzerOptions options)
        {

            Domain = options.Domain;
            Format = format;

            SplitterInterval = TimeSpan.FromSeconds(options.MicroTimingAmount / options.TimeResolution);
            BatchSize = Format.SampleRate / options.TimeResolution;
            Batch = new byte[BatchSize * Format.Size];
            DFT = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, options.SIMDEnabled, options.MaxParallelization))
                .ToArray();
            Splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, options))
                .ToArray();

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = options.BufferCapacity,
                CancellationToken = Cancel.Token,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                SingleProducerConstrained = true,
            };

            InputTask = Task.Run(() => RunInputAsync(Cancel.Token), Cancel.Token);
            BatcherTask = Task.Run(() => RunBatcherAsync(Cancel.Token), Cancel.Token);
            DFTBlock = new TransformBlock<SampleEvent<double>, SampleEvent<double>>(RunDFT, blockOptions);
            SplitterBlock = new TransformBlock<SampleEvent<double>, SampleEvent<double>>(RunSplitter, blockOptions);
            OutputBlock = new ActionBlock<SampleEvent<double>>(RunOutput, blockOptions);

            var linkOptions = new DataflowLinkOptions
            {
                //PropagateCompletion = true,
            };

            DFTBlock.LinkTo(SplitterBlock, linkOptions);
            SplitterBlock.LinkTo(OutputBlock, linkOptions);

        }

        static SampleEvent<T> RentEvent<T>(int sampleCount, int channelCount, DateTime time)
        {
            var samples = ArrayPool<T>.Shared.Rent(sampleCount * channelCount);
            return new SampleEvent<T>(samples, sampleCount, time);
        }

        static void ReturnEvent<T>(SampleEvent<T> evt)
        {
            ArrayPool<T>.Shared.Return(evt.Samples);
        }

        public void PostSamples(ReadOnlyMemory<byte> data)
        {
            if (Cancel.IsCancellationRequested)
            {
                return;
            }
            var evt = RentEvent<byte>(data.Length, 1, DateTime.Now);
            data.CopyTo(evt.Samples);
            InputQueue.Enqueue(evt);
            //SampleStream.Write(data.Span);
            //SampleStream.Flush();
        }

        public async Task StopAsync()
        {
            Cancel.Cancel();
            try
            {
                await InputTask;
                await BatcherTask;
                await DFTBlock.Completion;
                await SplitterBlock.Completion;
                await OutputBlock.Completion;
            }
            catch(OperationCanceledException)
            { }
            OnStopped?.Invoke(this, EventArgs.Empty);
        }

        public async Task RunInputAsync(CancellationToken cancel)
        {
            try
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();
                    //while(0 < Options.BufferCapacity && Options.BufferCapacity < InputQueue.Count)
                    //{
                    //    InputQueue.TryDequeue(out var _);
                    //}
                    if (!InputQueue.TryDequeue(out var input))
                    {
                        Thread.SpinWait(1);
                        continue;
                    }
                    await SamplePipe.Writer.WriteAsync(input.Samples[0..input.SampleCount]);
                    await SamplePipe.Writer.FlushAsync();
                    ReturnEvent(input);
                }
            }
            finally
            {
                await SamplePipe.Writer.CompleteAsync();
            }
        }

        public async Task RunBatcherAsync(CancellationToken cancel)
        {
            try
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();
                    var result = await SamplePipe.Reader.ReadAsync(cancel);
                    if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                    var input = result.Buffer;
                    long offset = 0;
                    for (; offset + Batch.Length < input.Length; offset += Batch.Length)
                    {
                        input.Slice(offset, Batch.Length).CopyTo(Batch);
                        var output = RentEvent<double>(BatchSize, Format.ChannelCount, DateTime.Now);
                        for (var c = 0; c < Format.ChannelCount; ++c)
                        {
                            var channel = output.GetChannel(c);
                            for (var i = 0; i < output.SampleCount; ++i)
                            {
                                channel.Span[i] = GetSample(Batch, i, c);
                            }
                        }
                        DFTBlock.Post(output);
                    }
                    SamplePipe.Reader.AdvanceTo(input.GetPosition(offset), input.End);
                }
            }
            finally { }
        }

        private double GetSample(ReadOnlySpan<byte> source, int sampleIndex, int channel)
        {
            var index = sampleIndex * Format.ChannelCount + channel;
            var offset = Format.Type.Size * index;
            var span = source[offset..];
            return Format.Type.ReadFrom(span);
        }

        public SampleEvent<double> RunDFT(SampleEvent<double> input)
        {
            var output = RentEvent<double>(Domain.Length, Format.ChannelCount, input.Time);
            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                var inputChannel = input.GetChannel(c);
                var outputChannel = output.GetChannel(c);
                DFT[c].Process(inputChannel);
                DFT[c].Output(outputChannel);
            }
            ReturnEvent(input);
            return output;
        }

        public SampleEvent<double> RunSplitter(SampleEvent<double> input)
        {
            var output = RentEvent<double>(4 * Domain.Length, Format.ChannelCount, input.Time);
            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
            }
            ReturnEvent(input);
            //while (SplitterTimer.Elapsed < SplitterInterval)
            //{
            //    Thread.SpinWait(100);
            //}
            //var remainingInterval = (SplitterInterval - SplitterTimer.Elapsed).TotalMilliseconds;
            //if(0 < remainingInterval)
            //{
            //    await MultimediaTimer.Delay((int)remainingInterval);
            //}
            SplitterTimer.Restart();
            return output;
        }

        public void RunOutput(SampleEvent<double> input)
        {
            OnSamplesAvailable?.Invoke(this, input);
            ReturnEvent(input);
        }

    }
}