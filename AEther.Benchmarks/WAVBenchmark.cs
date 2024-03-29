﻿using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Threading;

namespace AEther.Benchmarks
{
    public class WAVBenchmark
    {

        AnalyzerOptions Options;

        [Benchmark]
        public void RunA()
        {
            Options = new AnalyzerOptions
            {
            };
            Run();
        }

        [Benchmark]
        public void RunB()
        {
            Options = new AnalyzerOptions
            {
                MaxParallelization = 4
            };
            Run();
        }

        public void Run()
        {
            var path = Environment.CurrentDirectory;
#if !DEBUG
            path = Path.Join(path, "..", "..", "..", "..");
#endif
            path = Path.Join(path, "..", "..", "..", "..", "TestFiles", "test_sine.wav");
            path = new FileInfo(path).FullName;
            //Console.WriteLine(path);

            using var inputStream = File.OpenRead(path);
            using var outputStream = new MemoryStream();
            using var sampleSource = new WAVReader(inputStream);

            var session = new Analyzer(sampleSource.Format, Options);
            var outputDoubles = new double[4 * Options.Domain.Length];
            var outputBytes = new byte[sizeof(double) * outputDoubles.Length];
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            session.SamplesAnalyzed += async (obj, evt) =>
            {
                for (int c = 0; c < sampleSource.Format.ChannelCount; ++c)
                {
                    var src = evt.GetChannel(c);
                    src.CopyTo(outputDoubles);
                    Buffer.BlockCopy(outputDoubles, 0, outputBytes, 0, outputBytes.Length);
                    await outputStream.WriteAsync(outputBytes);
                }
            };
            //session.OnStopped += (obj, evt) => waitHandle.Set();

            sampleSource.Start();

        }

    }
}
