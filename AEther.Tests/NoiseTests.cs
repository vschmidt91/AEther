using NUnit.Framework;

using System.Collections;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System;

namespace AEther.Tests
{
    public class Tests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestWhiteNoise()
        {
            using var wav = File.OpenRead("data/pinknoise.wav");
            var sampleSource = new SampleReader(wav);
            var format = sampleSource.Format;

            var session = new Session(format);
            sampleSource.OnDataAvailable += (sender, data) =>
            {
                var evt = new DataEvent(data.Length, DateTime.Now);
                data.CopyTo(evt.Data);
                session.Writer.TryWrite(evt);
            };

            var sessionTask = Task.Run(() => session.RunAsync());
            sampleSource.Start();

            await foreach (var output in session.Reader.ReadAllAsync())
            {
                output.Dispose();
            }
            await sessionTask;

        }
    }
}