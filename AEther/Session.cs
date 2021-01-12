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

        public Session(SampleFormat format, SessionOptions? options = null)
        {
            Options = options ?? new SessionOptions();
            Domain = Options.Domain;
            Format = format;
        }

        public Pipe<PipeHandle, SampleEvent> CreateChain(int capacity = -1)
        {
            return CreateBatcher()
                .Buffer(capacity)
                .Chain(CreateDFT())
                .Buffer(capacity)
                .Chain(CreateSplitter())
                .Buffer(capacity);
        }

        public Pipe<PipeHandle, SampleEvent> CreateBatcher()
        {

            var batchSize = Format.SampleRate / Options.TimeResolution;
            var buffer = new byte[batchSize * Format.Size];

            async IAsyncEnumerable<SampleEvent> RunAsync(IAsyncEnumerable<PipeHandle> inputs)
            {
                await foreach(var input in inputs) 
                {
                    var samples = input.Data;
                    var sampleCount = samples.Length / Format.Size;
                    long offset;
                    for (offset = 0;  offset + batchSize < sampleCount; offset += batchSize)
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
                    input.AdvanceTo(samples.GetPosition(offset * Format.Size), samples.End);
                }
            }

            return RunAsync;

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

        public Pipe<SampleEvent, SampleEvent> CreateDFT()
        {

            var dft = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new DFTProcessor(Domain, Format.SampleRate, Options.UseSIMD, Options.MaxParallelization))
                .ToArray();

            async IAsyncEnumerable<SampleEvent> RunAsync(IAsyncEnumerable<SampleEvent> inputs)
            {
                await foreach (var input in inputs)
                {

                    var outputSamples = Pool.Rent(Format.ChannelCount * Domain.Count);
                    var output = new SampleEvent(outputSamples, Domain.Count, input.Time);

                    for (int c = 0; c < Format.ChannelCount; ++c)
                    {
                        dft[c].Process(input.GetChannel(c));
                        dft[c].Output(output.GetChannel(c).Span);
                    }

                    Pool.Return(input.Samples);
                    yield return output;
                }
            }

            return RunAsync;

        }

        public Pipe<SampleEvent, SampleEvent> CreateSplitter()
        {

            var splitter = Enumerable.Range(0, Format.ChannelCount)
                .Select(c => new Splitter(Domain, Options.TimeResolution, Options.FrequencyWindow, Options.TimeWindow, Options.NormalizerFloorRoom, Options.NormalizerHeadRoom))
                .ToArray();

            async IAsyncEnumerable<SampleEvent> RunAsync(IAsyncEnumerable<SampleEvent> inputs)
            {
                await foreach (var input in inputs)
                {
                    var outputSamples = Pool.Rent(4 * Format.ChannelCount * Domain.Count);
                    var output = new SampleEvent(outputSamples, 4 * Domain.Count, input.Time);

                    for (int c = 0; c < Format.ChannelCount; ++c)
                    {
                        splitter[c].Process(input.GetChannel(c).Span, output.GetChannel(c).Span);
                    }

                    Pool.Return(input.Samples);
                    yield return output;
                }
            }

            return RunAsync;

        }

    }
}