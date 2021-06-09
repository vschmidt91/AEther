using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public record SampleFormat
    (
        SampleType Type,
        int SampleRate,
        int ChannelCount
    )
    {

        public int Size => Type.Size * ChannelCount;

    }
}
