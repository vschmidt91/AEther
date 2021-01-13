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
        });

        [Benchmark]
        public Task RunB() => RunAsync(new SessionOptions
        {
            MaxParallelization = 4,
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
            var session = new Session(format, options);
            sampleSource.OnDataAvailable += (sender, evt) =>
            {
                session.Post(evt);
            };
            sampleSource.OnStopped += (sender, evt) =>
            {
                session.Complete();
            };


            var outputFloats = new float[4 * options.Domain.Count];
            var outputBytes = new byte[sizeof(float) * outputFloats.Length];

            sampleSource.Start();

            while(!session.Completion.IsCompleted)
            {
                SampleEvent output;
                try
                {
                    output = await session.ReceiveAsync(CancellationToken.None);
                }
                catch (InvalidOperationException) { break; }
                catch (TaskCanceledException) { break; }
                for (int c = 0; c < format.ChannelCount; ++c)
                {
                    var src = output.GetChannel(c);
                    src.CopyTo(outputFloats);
                    Buffer.BlockCopy(outputFloats, 0, outputBytes, 0, outputBytes.Length);
                    await outputStream.WriteAsync(outputBytes);
                }
                session.Pool.Return(output.Samples);
            }

            sampleSource.Dispose();

        }

    }
}
