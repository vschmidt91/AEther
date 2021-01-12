using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public class SampleEvent
    {

        static ArrayPool<float> Pool => ArrayPool<float>.Shared;

        float[] Samples;

        public readonly int SampleCount;
        public readonly DateTime Time;

        public SampleEvent(SampleEvent other, int sampleCount, DateTime time)
        {
            Samples = other.Samples;
            SampleCount = sampleCount;
            Time = time;
        }

        SampleEvent(float[] samples, int sampleCount, DateTime time)
        {
            Samples = samples;
            SampleCount = sampleCount;
            Time = time;
        }

        public Memory<float> GetChannel(int channelIndex)
        {
            var offset = channelIndex * SampleCount;
            return Samples.AsMemory(offset, SampleCount);
        }

        public static SampleEvent Rent(int channelCount, int sampleCount)
        {
            var samples = Pool.Rent(channelCount * sampleCount);
            return new SampleEvent(samples, sampleCount, DateTime.Now);
        }

        public void Return()
        {
            Pool.Return(Samples);
            Samples = null;
        }

    }
}
