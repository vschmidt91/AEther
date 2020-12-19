using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AEther
{
    public readonly struct SplitterEvent
    {

        static readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;

        public int ChannelCount => Channels.Length;

        readonly float[][] Channels;
        public int Length { get; }

        public readonly DateTime Time;
        
        public Memory<float> this[int c] => Channels[c].AsMemory(0, 4 * Length);

        public SplitterEvent(SplitterEvent other, int length, DateTime time)
        {
            Channels = other.Channels;
            Length = length;
            Time = time;
        }

        SplitterEvent(float[][] channels, int length, DateTime? time = default)
        {
            Channels = channels;
            Length = length;
            Time = time ?? DateTime.MinValue;
        }

        public static SplitterEvent Rent(int channelCount, int length)
        {
            var channels = Enumerable.Range(0, channelCount)
                .Select(c => Pool.Rent(4 * length))
                .ToArray();
            return new SplitterEvent(channels, length);
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
