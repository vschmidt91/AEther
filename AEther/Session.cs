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
        public SampleFormat Format => Source.Format;

        public readonly SessionOptions Options;
        public readonly SampleSource Source;
        public readonly Domain Domain;

        readonly int BatchSize;
        readonly byte[] Batch;
        readonly DFTProcessor[] DFT;
        readonly Splitter[] Splitter;

        readonly Pipe SamplePipe;
        readonly Channel<SampleEvent<double>> BatcherChannel;
        readonly Channel<SampleEvent<double>> DFTChannel;
        readonly Channel<SampleEvent<double>> SplitterChannel;

        public Session(SampleSource source, SessionOptions? options = null)
        {

            Options = options ?? new SessionOptions();
            Domain = Options.Domain;
            Source = source;

            BatchSize = Format.SampleRate / Options.TimeResolution;
            Batch = new byte[BatchSize * Format.Size];
            DFT = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, Options.UseSIMD, Options.MaxParallelization))
                .ToArray();
            Splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, Options.TimeResolution, Options.FrequencyWindow, Options.TimeWindow))
                .ToArray();

            SamplePipe = new Pipe();
            BatcherChannel = CreateChannel<SampleEvent<double>>();
            DFTChannel = CreateChannel<SampleEvent<double>>();
            SplitterChannel = CreateChannel<SampleEvent<double>>();

            Source.OnDataAvailable += (sender, data) =>
            {
                SamplePipe.Writer.Write(data.Span);
                //Task.Run(() => SamplePipe.Writer.FlushAsync()); 
                _ = SamplePipe.Writer.FlushAsync();
            };

            Source.OnStopped += async (sender, data) =>
            {
                await SamplePipe.Writer.FlushAsync();
                await SamplePipe.Writer.CompleteAsync();
            };

        }

        public async IAsyncEnumerable<SampleEvent<double>> RunAsync([EnumeratorCancellation] CancellationToken cancel = default)
        {

            var batcher = Task.Run(() => RunBatcherAsync(cancel), cancel);
            var dft = Task.Run(() => RunDFTAsync(cancel), cancel);
            var splitter = Task.Run(() => RunSplitterAsync(cancel), cancel);

            await foreach(var evt in RunThrottlerAsync(cancel))
            {
                yield return evt;
            }

            await batcher;
            await dft;
            await splitter;

        }

        Channel<T> CreateChannel<T>()
        {
            if(Options.BufferCapacity == -1)
            {
                return Channel.CreateUnbounded<T>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = true,
                });
            }
            else
            {
                return Channel.CreateBounded<T>(new BoundedChannelOptions(Options.BufferCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.DropOldest,
                });
            }
        }

        public async Task RunBatcherAsync(CancellationToken cancel)
        {
            await foreach (var input in SamplePipe.Reader.ReadAllAsync(cancel))
            {
                long offset = 0;
                for (; offset + Batch.Length < input.Data.Length; offset += Batch.Length)
                {
                    input.Data.Slice(offset, Batch.Length).CopyTo(Batch);
                    var output = new SampleEvent<double>(BatchSize, Format.ChannelCount, DateTime.Now);
                    for (var c = 0; c < Format.ChannelCount; ++c)
                    {
                        var channel = output.GetChannel(c);
                        for (var i = 0; i < output.SampleCount; ++i)
                        {
                            channel.Span[i] = GetSample(Batch, i, c);
                        }
                    }
                    BatcherChannel.Writer.TryWrite(output);
                    //await outputs.WriteAsync(output, cancel);
                }
                input.AdvanceTo(input.Data.GetPosition(offset), input.Data.End);
            }
            BatcherChannel.Writer.Complete();
        }

        private float GetSample(byte[] source, int sampleIndex, int channel)
        {
            var index = sampleIndex * Format.ChannelCount + channel;
            var offset = Format.Type.GetSize() * index;
            return Format.Type switch
            {
                SampleType.UInt16 => BitConverter.ToInt16(source, offset) / (float)short.MaxValue,
                SampleType.Float32 => BitConverter.ToSingle(source, offset),
                _ => throw new Exception(),
            };
        }

        public async Task RunDFTAsync(CancellationToken cancel)
        {
            await foreach (var input in BatcherChannel.Reader.ReadAllAsync(cancel))
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
                DFTChannel.Writer.TryWrite(output);
            }
            DFTChannel.Writer.Complete();

        }

        public async Task RunSplitterAsync(CancellationToken cancel)
        {
            await foreach (var input in DFTChannel.Reader.ReadAllAsync(cancel))
            {
                var output = new SampleEvent<double>(4 * Domain.Count, Format.ChannelCount, input.Time);

                for (var c = 0; c < Format.ChannelCount; ++c)
                {
                    Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
                }

                input.Dispose();
                SplitterChannel.Writer.TryWrite(output);
            }
            SplitterChannel.Writer.Complete();

        }

        public async IAsyncEnumerable<SampleEvent<double>> RunThrottlerAsync([EnumeratorCancellation] CancellationToken cancel)
        {
            var timer = Stopwatch.StartNew();
            var targetInterval = TimeSpan.FromSeconds(Options.TimingAmount / Options.TimeResolution);
            await foreach (var output in SplitterChannel.Reader.ReadAllAsync(cancel))
            {
                while (timer.Elapsed < targetInterval)
                {
                    Thread.SpinWait(Options.TimingSpinwaits);
                }
                timer.Restart();
                yield return output;
            }
        }

    }
}