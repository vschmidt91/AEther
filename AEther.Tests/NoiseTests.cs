using NUnit.Framework;

using System.Collections;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

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

            var pipe = new System.IO.Pipelines.Pipe();
            var session = new Session(format);
            sampleSource.OnDataAvailable += async (sender, evt) =>
            {
                await pipe.Writer.WriteAsync(evt);
            };

            var inputs = pipe.Reader.ReadAllAsync();
            sampleSource.Start();

            while (!session.Completion.IsCompleted)
            {
                var output = await session.ReceiveAsync(CancellationToken.None);
                //var channels = Enumerable.Range(0, output.ChannelCount);
                //var min = channels.Min(c => output[c].Span.Min());
                //var avg = output.Channels.Average(c => c.Average());
                //var max = output.Channels.Max(c => c.Max());
                //Assert.Greater(min, -30);
                //Assert.Less(min, -10);
                //Assert.Greater(avg, -20);
                //Assert.Less(avg, 0);
                //Assert.Greater(max, -10);
                //Assert.Less(max, 10);
                session.Pool.Return(output.Samples);
            }

        }
    }
}