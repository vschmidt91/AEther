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
            using var sampleSource = new SampleReader(wav);
            var session = new Session(sampleSource);

            var outputs = session.RunAsync();
            sampleSource.Start();
            await foreach (var output in outputs)
            {
                output.Dispose();
            }

        }
    }
}