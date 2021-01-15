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

namespace AEther
{
    public class Session
    {

        public double BatcherLatency { get; protected set; } = 0f;
        public double DFTLatency { get; protected set; } = 0f;
        public double SplitterLatency { get; protected set; } = 0f;

        public ChannelWriter<DataEvent> Writer => SampleChannel.Writer;
        public ChannelReader<SampleEvent> Reader => SplitterChannel.Reader;

        public readonly SessionOptions Options;
        public readonly SampleFormat Format;
        public readonly Domain Domain;

        readonly byte[] Batch;
        readonly DFTProcessor[] DFT;
        readonly Splitter[] Splitter;

        readonly Channel<DataEvent> SampleChannel;
        readonly Channel<SampleEvent> BatcherChannel;
        readonly Channel<SampleEvent> DFTChannel;
        readonly Channel<SampleEvent> SplitterChannel;

        public Session(SampleFormat format, SessionOptions? options = null)
        {

            Options = options ?? new SessionOptions();
            Domain = Options.Domain;
            Format = format;

            var batchSize = Format.SampleRate / Options.TimeResolution;
            Batch = new byte[batchSize * Format.Size];
            DFT = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, Options.UseSIMD, Options.MaxParallelization))
                .ToArray();
            Splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, Options.TimeResolution, Options.FrequencyWindow, Options.TimeWindow, Options.NormalizerFloorRoom, Options.NormalizerHeadRoom))
                .ToArray();

            SampleChannel = CreateChannel<DataEvent>();
            BatcherChannel = CreateChannel<SampleEvent>();
            DFTChannel = CreateChannel<SampleEvent>();
            SplitterChannel = CreateChannel<SampleEvent>();


        }

        public async Task RunAsync(CancellationToken cancel = default)
        {
            var batcherTask = Task.Run(async () =>
            {
                await foreach (var input in SampleChannel.Reader.ReadAllAsync(cancel))
                {
                    await RunBatcherAsync(input, BatcherChannel.Writer, cancel);
                }
                BatcherChannel.Writer.Complete();
            }, cancel);
            var dftTask = Task.Run(async () =>
            {
                await foreach (var input in BatcherChannel.Reader.ReadAllAsync(cancel))
                {
                    await RunDFTAsync(input, DFTChannel.Writer, cancel);
                }
                DFTChannel.Writer.Complete();
            }, cancel);
            var splitterTask = Task.Run(async () =>
            {
                await foreach (var input in DFTChannel.Reader.ReadAllAsync(cancel))
                {
                    await RunSplitterAsync(input, SplitterChannel.Writer, cancel);
                }
                SplitterChannel.Writer.Complete();
            }, cancel);
            await Task.WhenAll(batcherTask, dftTask, splitterTask);
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

        public async ValueTask RunBatcherAsync(DataEvent input, ChannelWriter<SampleEvent> outputs, CancellationToken cancel)
        {
            var batchSize = Batch.Length / Format.Size;
            for (var offset = 0; offset + Batch.Length < input.Length; offset += Batch.Length)
            {
                Array.Copy(input.Data, offset, Batch, 0, Batch.Length);
                var output = new SampleEvent(batchSize, Format.ChannelCount, input.Time);
                for (var c = 0; c < Format.ChannelCount; ++c)
                {
                    var channel = output.GetChannel(c);
                    for (var i = 0; i < output.SampleCount; ++i)
                    {
                        channel.Span[i] = GetSample(Batch, i, c);
                    }
                }
                await outputs.WriteAsync(output, cancel);
            }
            BatcherLatency += .01 * ((DateTime.Now - input.Time).TotalMilliseconds - BatcherLatency);
            input.Dispose();
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

        public async ValueTask RunDFTAsync(SampleEvent input, ChannelWriter<SampleEvent> outputs, CancellationToken cancel)
        {
            var output = new SampleEvent(Domain.Count, Format.ChannelCount, input.Time);

            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                DFT[c].Process(input.GetChannel(c));
                DFT[c].Output(output.GetChannel(c));
            }
            //Task RunAsync(DFTProcessor dft, int channel)
            //{
            //    void Run()
            //    {
            //        dft.Process(input.GetChannel(channel));
            //        dft.Output(output.GetChannel(channel).Span);
            //    }
            //    return Task.Run(Run, cancel);
            //}
            //await Task.WhenAll(DFT.Select(RunAsync));

            input.Dispose();
            DFTLatency += .01 * ((DateTime.Now - input.Time).TotalMilliseconds - DFTLatency);
            await outputs.WriteAsync(output, cancel);
        }

        public async ValueTask RunSplitterAsync(SampleEvent input, ChannelWriter<SampleEvent> outputs, CancellationToken cancel)
        {
            var output = new SampleEvent(4 * Domain.Count, Format.ChannelCount, input.Time);

            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
            }
            //Task RunAsync(Splitter splitter, int channel)
            //{
            //    void Run()
            //    {
            //        splitter.Process(input.GetChannel(channel).Span, output.GetChannel(channel).Span);
            //    }
            //    return Task.Run(Run, cancel);
            //}
            //await Task.WhenAll(Splitter.Select(RunAsync));

            input.Dispose();
            SplitterLatency += .01 * ((DateTime.Now - input.Time).TotalMilliseconds - SplitterLatency);
            await outputs.WriteAsync(output, cancel);
        }

    }
}