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
                await foreach (var input in SampleChannel.Reader.ReadAllAsync())
                {
                    foreach (var output in RunBatcher(input))
                    {
                        await BatcherChannel.Writer.WriteAsync(output);
                    }
                }
                BatcherChannel.Writer.Complete();
            });
            var dftTask = Task.Run(async () =>
            {
                await foreach (var input in BatcherChannel.Reader.ReadAllAsync())
                {
                    var output = RunDFT(input);
                    await DFTChannel.Writer.WriteAsync(output);
                }
                DFTChannel.Writer.Complete();
            });
            var splitterTask = Task.Run(async () =>
            {
                await foreach (var input in DFTChannel.Reader.ReadAllAsync())
                {
                    var output = RunSplitter(input);
                    await SplitterChannel.Writer.WriteAsync(output);
                }
                SplitterChannel.Writer.Complete();
            });
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

        public IEnumerable<SampleEvent> RunBatcher(DataEvent input)
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
                yield return output;

            }
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

        public SampleEvent RunDFT(SampleEvent input)
        {
            var output = new SampleEvent(Domain.Count, Format.ChannelCount, input.Time);
            for (int c = 0; c < Format.ChannelCount; ++c)
            {
                DFT[c].Process(input.GetChannel(c));
                DFT[c].Output(output.GetChannel(c).Span);
            }
            input.Dispose();
            return output;
        }

        public SampleEvent RunSplitter(SampleEvent input)
        {
            var output = new SampleEvent(4 * Domain.Count, Format.ChannelCount, input.Time);
            for (int c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c).Span, output.GetChannel(c).Span);
            }
            input.Dispose();
            return output;
        }

    }
}