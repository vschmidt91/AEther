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
    public class Session
    {

        public event EventHandler<SampleEvent<double>>? OnSamplesAvailable;
        public event EventHandler? OnStopped;

        public SampleFormat Format => Source.Format;

        public readonly SessionOptions Options;
        public readonly SampleSource Source;
        public readonly Domain Domain;

        readonly int BatchSize;
        readonly byte[] Batch;
        readonly DFTProcessor[] DFT;
        readonly Splitter[] Splitter;

        readonly Pipe SamplePipe;
        readonly CancellationTokenSource Cancel;
        readonly Task BatcherTask;
        readonly TransformBlock<SampleEvent<double>, SampleEvent<double>> DFTBlock;
        readonly TransformBlock<SampleEvent<double>, SampleEvent<double>> SplitterBlock;
        readonly ActionBlock<SampleEvent<double>> OutputBlock;

        public Session(SampleSource source, SessionOptions? options = null)
        {

            Options = options ?? new SessionOptions();
            Domain = Options.Domain;
            Source = source;

            BatchSize = Format.SampleRate / Options.TimeResolution;
            Batch = new byte[BatchSize * Format.Size];
            DFT = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, Options.SIMDEnabled, Options.MaxParallelization))
                .ToArray();
            Splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, Options))
                .ToArray();

            SamplePipe = new Pipe();

            Source.OnDataAvailable += Source_OnDataAvailable;
            Source.OnStopped += Source_OnStopped;

            Cancel = new CancellationTokenSource();

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = Options.BufferCapacity,
                CancellationToken = Cancel.Token,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                SingleProducerConstrained = true,
            };

            BatcherTask = Task.Run(() => RunBatcherAsync(Cancel.Token), Cancel.Token);
            DFTBlock = new TransformBlock<SampleEvent<double>, SampleEvent<double>>(RunDFT, blockOptions);
            SplitterBlock = new TransformBlock<SampleEvent<double>, SampleEvent<double>>(RunSplitter, blockOptions);
            OutputBlock = new ActionBlock<SampleEvent<double>>(RunOutput, blockOptions);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            //BatcherBlock.LinkTo(DFTBlock, linkOptions);
            DFTBlock.LinkTo(SplitterBlock, linkOptions);
            SplitterBlock.LinkTo(OutputBlock, linkOptions);

        }

        public async Task PostSamplesAsync(ReadOnlyMemory<byte> samples)
        {
            //SampleStream.Write(samples.Span);
            //SampleStream.Flush();
            await SamplePipe.Writer.WriteAsync(samples, Cancel.Token);
            await SamplePipe.Writer.FlushAsync(Cancel.Token);
        }

        void Source_OnDataAvailable(object? sender, ReadOnlyMemory<byte> data)
        {
            if (Cancel.IsCancellationRequested)
            {
                return;
            }
        }

        async void Source_OnStopped(object? sender, Exception? error)
        {
            if (error != null)
            {
                throw error;
            }
            else
            {
                await StopAsync();
            }
        }

        public async Task StopAsync()
        {
            Cancel.Cancel();
            try
            {
                await BatcherTask;
                await DFTBlock.Completion;
                await SplitterBlock.Completion;
                await OutputBlock.Completion;
            }
            catch(OperationCanceledException)
            { }
            OnStopped?.Invoke(this, EventArgs.Empty);
        }

        public async Task RunBatcherAsync(CancellationToken cancel)
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
                    var output = new SampleEvent<double>(BatchSize, Format.ChannelCount, DateTime.Now);
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

        private double GetSample(byte[] source, int sampleIndex, int channel)
        {
            var index = sampleIndex * Format.ChannelCount + channel;
            var offset = Format.Type.GetSize() * index;
            return Format.Type switch
            {
                SampleType.UInt16 => BitConverter.ToInt16(source, offset) / (double)short.MaxValue,
                SampleType.Float32 => BitConverter.ToSingle(source, offset),
                _ => throw new Exception(),
            };
        }

        public SampleEvent<double> RunDFT(SampleEvent<double> input)
        {
            var output = new SampleEvent<double>(Domain.Count, Format.ChannelCount, input.Time);
            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                var inputChannel = input.GetChannel(c);
                var outputChannel = output.GetChannel(c);
                DFT[c].Process(inputChannel);
                DFT[c].Output(outputChannel);
            }
            input.Dispose();
            return output;
        }

        Stopwatch SplitterTimer = Stopwatch.StartNew();
        TimeSpan SplitterInterval => TimeSpan.FromSeconds(1.0 / Options.TimeResolution);

        public SampleEvent<double> RunSplitter(SampleEvent<double> input)
        {
            var output = new SampleEvent<double>(4 * Domain.Count, Format.ChannelCount, input.Time);
            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
            }
            input.Dispose();
            SplitterTimer.Stop();
            if (Options.MicroTimingEnabled && SplitterTimer.Elapsed < SplitterInterval)
            {
                var remainingInterval = (uint)(SplitterInterval - SplitterTimer.Elapsed).TotalMilliseconds;
                MultimediaTimer.Sleep(remainingInterval);
            }
            SplitterTimer.Restart();
            return output;
        }

        public void RunOutput(SampleEvent<double> input)
        {
            OnSamplesAvailable?.Invoke(this, input);
        }

    }
}