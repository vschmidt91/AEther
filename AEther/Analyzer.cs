using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AEther
{
    public class Analyzer
    {

        public event EventHandler<SampleEvent<double>>? SamplesAnalyzed;

        public readonly SampleFormat Format;
        public readonly Domain Domain;
        public readonly Task Completion;
        public readonly AnalyzerOptions Options;

        readonly int BatchSize;
        readonly byte[] Batch;
        readonly DFTProcessor[] DFT;
        readonly Splitter[] Splitter;
        readonly Pipe SamplePipe = new();
        readonly CancellationTokenSource Cancel = new();
        readonly TransformBlock<SampleEvent<double>, SampleEvent<double>> DFTBlock;
        readonly ActionBlock<SampleEvent<double>> SplitterBlock;
        readonly ActionBlock<SampleEvent<byte>> InputBlock;
        readonly ConcurrentQueue<SampleEvent<double>> OutputBuffer = new();
        readonly Task BatcherTask;

        readonly MultimediaTimer OutputTimer;

        public Analyzer(SampleFormat format, AnalyzerOptions options)
        {

            Domain = options.Domain;
            Format = format;
            BatchSize = Format.SampleRate / options.TimeResolution;
            Batch = new byte[BatchSize * Format.Size];
            DFT = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, options.SIMDEnabled, options.MaxParallelization))
                .ToArray();
            Splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, options))
                .ToArray();
            Options = options;

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = options.BufferCapacity,
                CancellationToken = Cancel.Token,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                SingleProducerConstrained = false,
            };

            InputBlock = new ActionBlock<SampleEvent<byte>>(RunInputAsync, blockOptions);
            InputBlock.Completion.ContinueWith(_ => SamplePipe.Writer.CompleteAsync());
            BatcherTask = Task.Run(() => RunBatcherAsync(Cancel.Token), Cancel.Token);
            DFTBlock = new TransformBlock<SampleEvent<double>, SampleEvent<double>>(RunDFT, blockOptions);
            SplitterBlock = new ActionBlock<SampleEvent<double>>(RunSplitter, blockOptions);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            DFTBlock.LinkTo(SplitterBlock, linkOptions);
            //SplitterBlock.LinkTo(OutputBlock, linkOptions);

            Completion = Task.WhenAll(
                InputBlock.Completion,
                BatcherTask,
                DFTBlock.Completion,
                SplitterBlock.Completion);


            var interval = (int)TimeSpan.FromSeconds(1.0 / options.TimeResolution).TotalMilliseconds;

            OutputTimer = new()
            {
                Resolution = 0,
                Interval = interval,
            };


        }

        public void Start()
        {
            if (0 < OutputTimer.Interval)
            {
                OutputTimer.Elapsed += Timer_Elapsed;
                OutputTimer.Start();
            }
        }

        private void Timer_Elapsed(object? sender, EventArgs e)
        {
            if (OutputBuffer.TryDequeue(out var output))
            {
                PublishOutput(output);
            }
            while (0 < Options.BufferCapacity
                && Options.BufferCapacity < OutputBuffer.Count
                && OutputBuffer.TryDequeue(out _)) ;
        }

        void PublishOutput(SampleEvent<double> output)
        {
            SamplesAnalyzed?.Invoke(this, output);
            ReturnEvent(output);
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
            InputBlock.Post(evt);
        }

        public void Stop()
        {
            if (OutputTimer.IsRunning)
            {
                OutputTimer.Elapsed -= Timer_Elapsed;
                OutputTimer.Stop();
            }
            Cancel.Cancel();
        }

        public async Task RunInputAsync(SampleEvent<byte> evt)
        {
            await SamplePipe.Writer.WriteAsync(evt.Samples.AsMemory(0, evt.SampleCount));
            await SamplePipe.Writer.FlushAsync();
            ReturnEvent(evt);
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
            finally
            {
                DFTBlock.Complete();
            }
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

        public void RunSplitter(SampleEvent<double> input)
        {
            var output = RentEvent<double>(4 * Domain.Length, Format.ChannelCount, input.Time);
            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
            }
            ReturnEvent(input);
            if (OutputTimer.IsRunning)
            {
                OutputBuffer.Enqueue(output);
            }
            else
            {
                PublishOutput(output);
            }
        }

    }
}