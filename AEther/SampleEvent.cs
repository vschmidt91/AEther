using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public readonly struct SampleEvent
    {

        public static readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;

        readonly float[][] Channels;


        public int ChannelCount => Channels.Length;
        public int Length { get; }

        public Memory<float> this[int c] => Channels[c].AsMemory(0, Length);

        public readonly DateTime Time;

        public SampleEvent(SampleEvent other, int length, DateTime time)
        {
            Channels = other.Channels;
            Length = length;
            Time = time;
        }

        SampleEvent(float[][] channels, int length, DateTime? time = default)
        {
            Channels = channels;
            Length = length;
            Time = time ?? DateTime.MinValue;
        }

        public static SampleEvent Rent(int channelCount, int length)
        {
            var channels = Enumerable.Range(0, channelCount)
                .Select(c => Pool.Rent(length))
                .ToArray();
            return new SampleEvent(channels, length);
        }

        public void Return()
        {
            foreach(var channel in Channels)
            {
                Pool.Return(channel);
            }
        }

    }
}
