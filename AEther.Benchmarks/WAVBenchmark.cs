using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AEther.Benchmarks
{
    public class WAVBenchmark
    {

        [Benchmark]
        public Task RunA() => RunAsync(new SessionOptions
        {
            FrequencyResolution = 1,
        });

        [Benchmark]
        public Task RunB() => RunAsync(new SessionOptions
        {
            FrequencyResolution = 2,
        });

        public static async Task RunAsync(SessionOptions options)
        {

            //var path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            var path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "..", "..", "..", "TestFiles", "test_sine.wav");
            var inputStream = File.OpenRead(path);
            var outputStream = new MemoryStream();

            var header = WAVHeader.FromStream(inputStream);
            var format = header.GetSampleFormat();
            var sampleSource = new SampleReader(inputStream);
            var pipe = new System.IO.Pipelines.Pipe();
            sampleSource.OnDataAvailable += async (sender, evt) =>
            {
                await pipe.Writer.WriteAsync(evt);
            };

            var session = new Session(format, options);
            var chain = session.CreateChain();

            var outputFloats = new float[4 * options.Domain.Count];
            var outputBytes = new byte[sizeof(float) * outputFloats.Length];

            var inputs = pipe.Reader.ReadAllAsync();
            sampleSource.Start();
            await foreach (var output in chain(inputs))
            {
                for (int c = 0; c < output.ChannelCount; ++c)
                {
                    var src = output[c];
                    src.CopyTo(outputFloats);
                    Buffer.BlockCopy(outputFloats, 0, outputBytes, 0, outputBytes.Length);
                    await outputStream.WriteAsync(outputBytes);
                }
                output.Return();
            }

        }

    }
}
