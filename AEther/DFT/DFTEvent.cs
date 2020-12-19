using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AEther
{

    public readonly struct DFTEvent
    {

        static readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;

        public readonly float[][] Channels;
        public readonly DateTime Time;

        public int ChannelCount => Channels.Length;
        public Memory<float> this[int i] => Channels[i].AsMemory(0, Length);

        public int Length { get; }

        public DFTEvent(DFTEvent other, int length, DateTime time)
        {
            Channels = other.Channels;
            Length = length;
            Time = time;
        }

        DFTEvent(float[][] channels, int length, DateTime? time = default)
        {
            Channels = channels;
            Length = length;
            Time = time ?? DateTime.MinValue;
        }

        public static DFTEvent Rent(int channelCount, int length)
        {
            var channels = Enumerable.Range(0, channelCount)
                .Select(c => Pool.Rent(length))
                .ToArray();
            return new DFTEvent(channels, length);
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