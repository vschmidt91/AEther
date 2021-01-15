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
            var inputStream = File.OpenRead(path);
            var outputStream = new MemoryStream();

            var header = WAVHeader.FromStream(inputStream);
            var format = header.GetSampleFormat();
            var sampleSource = new SampleReader(inputStream);
            var session = new Session(format, Options);
            sampleSource.OnDataAvailable += (sender, data) =>
            {
                var evt = new DataEvent(data.Length, DateTime.Now);
                data.CopyTo(evt.Data);
                session.Writer.TryWrite(evt);
            };
            sampleSource.OnStopped += (sender, evt) =>
            {
                session.Writer.TryComplete();
            };


            var outputFloats = new float[4 * Options.Domain.Count];
            var outputBytes = new byte[sizeof(float) * outputFloats.Length];

            var sessionTask = Task.Run(() => session.RunAsync());
            sampleSource.Start();

            await foreach(var output in session.Reader.ReadAllAsync())
            {
                for (int c = 0; c < format.ChannelCount; ++c)
                {
                    var src = output.GetChannel(c);
                    src.CopyTo(outputFloats);
                    Buffer.BlockCopy(outputFloats, 0, outputBytes, 0, outputBytes.Length);
                    await outputStream.WriteAsync(outputBytes);
                }
                output.Dispose();
            }
            await sessionTask;

            sampleSource.Dispose();

        }

    }
}
