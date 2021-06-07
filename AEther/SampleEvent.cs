using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public class SampleEvent<T> : IDisposable
    {

        static ArrayPool<T> Pool => ArrayPool<T>.Shared;

        public readonly T[] Samples;
        public readonly int SampleCount;
        public readonly DateTime Time;

        public SampleEvent(int sampleCount, int channelCount, DateTime time)
        {
            Samples = Pool.Rent(sampleCount * channelCount);
            SampleCount = sampleCount;
            Time = time;
        }

        public Memory<T> GetChannel(int channelIndex)
        {
            var offset = channelIndex * SampleCount;
            return Samples.AsMemory(offset, SampleCount);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Pool.Return(Samples);
        }

    }
}
