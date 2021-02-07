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
        readonly MovingQuantileEstimator Ceiling;

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
            Ceiling = new MovingQuantileEstimator(.9f, .01f);

        }

        public async IAsyncEnumerable<SampleEvent> RunAsync([EnumeratorCancellation] CancellationToken cancel = default)
        {

            var samplePipe = new Pipe();
            var batcherChannel = CreateChannel<SampleEvent>();
            var dftChannel = CreateChannel<SampleEvent>();
            var splitterChannel = CreateChannel<SampleEvent>();

            Source.OnDataAvailable += (sender, data) =>
            {
                samplePipe.Writer.Write(data.Span);
                _ = samplePipe.Writer.FlushAsync();
            };

            Source.OnStopped += async (sender, data) =>
            {
                await samplePipe.Writer.FlushAsync();
                await samplePipe.Writer.CompleteAsync();
            };

            var batcherTask = Task.Run(async () =>
            {
                await foreach (var input in samplePipe.Reader.ReadAllAsync(cancel))
                {
                    RunBatcher(input, batcherChannel.Writer);
                }
                batcherChannel.Writer.Complete();
            }, cancel);
            var dftTask = Task.Run(async () =>
            {
                await foreach (var input in batcherChannel.Reader.ReadAllAsync(cancel))
                {
                    RunDFT(input, dftChannel.Writer);
                }
                dftChannel.Writer.Complete();
            }, cancel);
            var splitterTask = Task.Run(async () =>
            {
                await foreach (var input in dftChannel.Reader.ReadAllAsync(cancel))
                {
                    RunSplitter(input, splitterChannel.Writer);
                }
                splitterChannel.Writer.Complete();
            }, cancel);

            //return splitterChannel.Reader.ReadAllAsync(cancel);

            await foreach (var output in splitterChannel.Reader.ReadAllAsync(cancel))
            {
                yield return output;
            }

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

        public void RunBatcher(PipeHandle input, ChannelWriter<SampleEvent> outputs)
        {
            long offset = 0;
            for (; offset + Batch.Length < input.Data.Length; offset += Batch.Length)
            {
                input.Data.Slice(offset, Batch.Length).CopyTo(Batch);
                var output = new SampleEvent(BatchSize, Format.ChannelCount, DateTime.Now);
                for (var c = 0; c < Format.ChannelCount; ++c)
                {
                    var channel = output.GetChannel(c);
                    for (var i = 0; i < output.SampleCount; ++i)
                    {
                        channel.Span[i] = GetSample(Batch, i, c);
                    }
                }
                outputs.TryWrite(output);
                //await outputs.WriteAsync(output, cancel);
            }
            input.AdvanceTo(input.Data.GetPosition(offset), input.Data.End);
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

        public void RunDFT(SampleEvent input, ChannelWriter<SampleEvent> outputs)
        {

            var ceiling = Ceiling.Filter(input.Samples[0..input.SampleCount].Select(Math.Abs).Max());

            var output = new SampleEvent(Domain.Count, Format.ChannelCount, input.Time);

            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                var inputChannel = input.GetChannel(c);
                var outputChannel = output.GetChannel(c);

                //if (0 < ceiling)
                //{
                //    for (var i = 0; i < inputChannel.Length; ++i)
                //    {
                //        inputChannel.Span[i] *= 1f / ceiling;
                //    }
                //}

                DFT[c].Process(inputChannel);
                DFT[c].Output(outputChannel);
            }

            input.Dispose();
            outputs.TryWrite(output);
        }

        public void RunSplitter(SampleEvent input, ChannelWriter<SampleEvent> outputs)
        {
            var output = new SampleEvent(4 * Domain.Count, Format.ChannelCount, input.Time);

            for (var c = 0; c < Format.ChannelCount; ++c)
            {
                Splitter[c].Process(input.GetChannel(c), output.GetChannel(c));
            }

            input.Dispose();
            outputs.TryWrite(output);
        }

    }
}