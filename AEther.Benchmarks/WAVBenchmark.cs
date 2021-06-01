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

        SessionOptions Options;

        [Benchmark]
        public async Task RunA()
        {
            Options = new SessionOptions();
            await RunAsync();
        }

        [Benchmark]
        public async Task RunB()
        {
            Options = new SessionOptions
            {
                MaxParallelization = 4
            };
            await RunAsync();
        }

        public async Task RunAsync()
        {

            //var path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "TestFiles", "test_input.wav");
            var path = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "..", "..", "..", "TestFiles", "test_sine.wav");
            path = new FileInfo(path).FullName;
            using var inputStream = File.OpenRead(path);
            using var outputStream = new MemoryStream();
            using var sampleSource = new WAVReader(inputStream);

            var session = new Session(sampleSource, Options);
            var outputDoubles = new double[4 * Options.Domain.Count];
            var outputBytes = new byte[sizeof(double) * outputDoubles.Length];

            var outputs = session.RunAsync();
            sampleSource.Start();

            await foreach(var output in outputs)
            {
                for (int c = 0; c < sampleSource.Format.ChannelCount; ++c)
                {
                    var src = output.GetChannel(c);
                    src.CopyTo(outputDoubles);
                    Buffer.BlockCopy(outputDoubles, 0, outputBytes, 0, outputBytes.Length);
                    await outputStream.WriteAsync(outputBytes);
                }
                output.Dispose();
            }

        }

    }
}
