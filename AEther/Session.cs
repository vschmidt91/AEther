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

        public readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;
        public readonly SessionOptions Options;
        public readonly SampleFormat Format;
        public readonly Domain Domain;

        readonly TransformManyBlock<ReadOnlyMemory<byte>, SampleEvent> Batcher;
        readonly TransformBlock<SampleEvent, SampleEvent> DFT;
        readonly TransformBlock<SampleEvent, SampleEvent> Splitter;

        public Session(SampleFormat format, SessionOptions? options = null)
        {

            Options = options ?? new SessionOptions();
            Domain = Options.Domain;
            Format = format;

            var executionOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = Options.BufferCapacity,
                CancellationToken = CancellationToken.None,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                SingleProducerConstrained = true,
            };

            Batcher = CreateBatcher(executionOptions);
            DFT = CreateDFT(executionOptions);
            Splitter = CreateSplitter(executionOptions);

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            Batcher.LinkTo(DFT, linkOptions);
            DFT.LinkTo(Splitter, linkOptions);

        }

        public bool Post(ReadOnlyMemory<byte> samples) => Batcher.Post(samples);

        public void Complete() => Batcher.Complete();

        public Task Completion => Splitter.Completion;
        public Task<SampleEvent> ReceiveAsync(CancellationToken cancel) => Splitter.ReceiveAsync(cancel);

        public TransformManyBlock<ReadOnlyMemory<byte>, SampleEvent> CreateBatcher(ExecutionDataflowBlockOptions options)
        {

            IEnumerable<SampleEvent> Transform(ReadOnlyMemory<byte> samples)
            {
                var batchSize = Format.SampleRate / Options.TimeResolution;
                var buffer = ArrayPool<byte>.Shared.Rent(batchSize * Format.Size);
                var sampleCount = samples.Length / Format.Size;
                int offset;
                for (offset = 0; offset + batchSize < sampleCount; offset += batchSize)
                {

                    var batch = samples.Slice(offset * Format.Size, batchSize * Format.Size);
                    batch.CopyTo(buffer);

                    var outputSamples = Pool.Rent(Format.ChannelCount * batchSize);
                    var output = new SampleEvent(outputSamples, batchSize, DateTime.Now);
                    for (var c = 0; c < Format.ChannelCount; ++c)
                    {
                        var channel = output.GetChannel(c);
                        for (var i = 0; i < batchSize; ++i)
                        {
                            channel.Span[i] = GetSample(buffer, i, c);
                        }
                    }

                    yield return output;

                }
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TransformManyBlock<ReadOnlyMemory<byte>, SampleEvent>(Transform, options);

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

        public TransformBlock<SampleEvent, SampleEvent> CreateDFT(ExecutionDataflowBlockOptions options)
        {

            var dft = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, Options.UseSIMD, Options.MaxParallelization))
                .ToArray();

            SampleEvent Transform(SampleEvent input)
            {
                var outputSamples = Pool.Rent(Format.ChannelCount * Domain.Count);
                var output = new SampleEvent(outputSamples, Domain.Count, input.Time);

                for (int c = 0; c < Format.ChannelCount; ++c)
                {
                    dft[c].Process(input.GetChannel(c));
                    dft[c].Output(output.GetChannel(c).Span);
                }

                Pool.Return(input.Samples);
                return output;
            }

            return new TransformBlock<SampleEvent, SampleEvent>(Transform, options);

        }

        public TransformBlock<SampleEvent, SampleEvent> CreateSplitter(ExecutionDataflowBlockOptions options)
        {

            var splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, Options.TimeResolution, Options.FrequencyWindow, Options.TimeWindow, Options.NormalizerFloorRoom, Options.NormalizerHeadRoom))
                .ToArray();

            SampleEvent Transform(SampleEvent input)
            {
                var outputSamples = Pool.Rent(4 * Format.ChannelCount * Domain.Count);
                var output = new SampleEvent(outputSamples, 4 * Domain.Count, input.Time);

                for (int c = 0; c < Format.ChannelCount; ++c)
                {
                    splitter[c].Process(input.GetChannel(c).Span, output.GetChannel(c).Span);
                }

                Pool.Return(input.Samples);
                return output;
            }

            return new TransformBlock<SampleEvent, SampleEvent>(Transform, options);

        }

    }
}