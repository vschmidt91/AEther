﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public readonly struct SampleFormat
    {

        public readonly SampleType Type;
        public readonly int SampleRate;
        public readonly int ChannelCount;

        public SampleFormat(SampleType type, int sampleRate, int channelCount)
        {
            Type = type;
            SampleRate = sampleRate;
            ChannelCount = channelCount;
        }

        public int Size => Type.GetSize() * ChannelCount;

    }
}
