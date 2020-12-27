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

        readonly Configuration Configuration;
        readonly SampleFormat Format;

        public Session(Configuration configuration, SampleFormat format)
        {
            Configuration = configuration;
            Format = format;
        }

        public Pipe<PipeHandle, SampleEvent> CreateBatcher()
            => CreateBatcher(Format, Format.SampleRate / Configuration.TimeResolution);

        public static Pipe<PipeHandle, SampleEvent> CreateBatcher(SampleFormat format, int batchSize)
        {

            var buffer = new byte[batchSize * format.Size];
            var bufferFloat = new float[batchSize * format.ChannelCount];

            async IAsyncEnumerable<SampleEvent> RunAsync(IAsyncEnumerable<PipeHandle> inputs)
            {
                await foreach(var input in inputs)
                {
                    var sampleCount = input.Data.Length / format.Size;
                    long offset = 0;
                    while (offset + batchSize < sampleCount)
                    {

                        var output = SampleEvent.Rent(format.ChannelCount, batchSize);

                        input.Data.Slice(offset * format.Size, batchSize * format.Size).CopyTo(buffer);
                        Buffer.BlockCopy(buffer, 0, bufferFloat, 0, buffer.Length);

                        for (var i = 0; i < batchSize; ++i)
                        {
                            for (var c = 0; c < format.ChannelCount; ++c)
                            {
                                output[c].Span[i] = GetSample(buffer, format, i, c);
                                if (float.IsNaN(output[c].Span[i]))
                                    throw new Exception();
                            }
                        }

                        yield return new SampleEvent(output, batchSize, DateTime.Now);
                        offset += batchSize;


                    }
                    input.Reader.AdvanceTo(input.Data.GetPosition(offset * format.Size), input.Data.End);
                }
            }

            return RunAsync;

        }

        private static float GetSample(ReadOnlyMemory<byte> source, SampleFormat format, int offset, int channel)
        {
            var i = offset * format.ChannelCount + channel;
            var size = format.Type.GetSize();
            var element = source.Slice(size * i, size).Span;
            return format.Type switch
            {
                SampleType.UInt16 => BitConverter.ToInt16(element) / 32768f,
                SampleType.Float32 => BitConverter.ToSingle(element),
                _ => throw new Exception(),
            };
        }

        public Pipe<SampleEvent, DFTEvent> CreateDFT()
            => CreateDFT(Configuration.Domain, Format, Configuration.UseSIMD, Configuration.MaxParallelization);

        public static Pipe<SampleEvent, DFTEvent> CreateDFT(Domain domain, SampleFormat format, bool useSIMD = true, int maxParallelism = -1)
        {

            var dft = Enumerable.Range(0, format.ChannelCount)
                .Select(c => new DFTProcessor(domain, format.SampleRate, useSIMD, maxParallelism))
                .ToArray();

            async IAsyncEnumerable<DFTEvent> RunAsync(IAsyncEnumerable<SampleEvent> inputs)
            {
                await foreach (var input in inputs)
                {
                    var output = DFTEvent.Rent(format.ChannelCount, domain.Count);
                    await Task.WhenAll(Enumerable.Range(0, format.ChannelCount)
                        .Select(c => Task.Run(() =>
                        {
                            dft[c].Process(input[c]);
                            dft[c].Output(output[c].Span);
                        })));
                    //for (int c = 0; c < format.ChannelCount; ++c)
                    //{
                    //    dft[c].Process(input[c]);
                    //    dft[c].Output(output[c].Span);
                    //}
                    input.Return();
                    yield return new DFTEvent(output, domain.Count, input.Time);
                }
            }

            return RunAsync;

        }

        public Pipe<DFTEvent, SplitterEvent> CreateSplitter()
            => CreateSplitter(Configuration.Domain, Format.ChannelCount, Configuration.TimeResolution, Configuration.FrequencyWindow, Configuration.TimeWindow, Configuration.NormalizerFloorRoom, Configuration.NormalizerHeadRoom);

        public static Pipe<DFTEvent, SplitterEvent> CreateSplitter(Domain domain, int channelCount, float timeResolution, float frequencyWindow, float timeWindow, float floorRoom, float headRoom)
        {

            var splitter = Enumerable.Range(0, channelCount)
                .Select(c => new Splitter(domain, timeResolution, frequencyWindow, timeWindow, floorRoom, headRoom))
                .ToArray();

            async IAsyncEnumerable<SplitterEvent> RunAsync(IAsyncEnumerable<DFTEvent> inputs)
            {
                await foreach (var input in inputs)
                {
                    var output = SplitterEvent.Rent(channelCount, domain.Count);
                    await Task.WhenAll(Enumerable.Range(0, channelCount)
                        .Select(c => Task.Run(() =>
                        {
                            splitter[c].Process(input[c].Span, output[c].Span);
                        })));
                    //for(int c = 0; c < channelCount; ++c)
                    //{
                    //    splitter[c].Process(input[c].Span, output[c].Span);
                    //}
                    input.Return();
                    yield return new SplitterEvent(output, input.Length, input.Time);
                }
            }

            return RunAsync;

        }

    }
}