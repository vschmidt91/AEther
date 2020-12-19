using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AEther
{
    public readonly struct WAVHeader
    {

        public readonly RIFFHeader RIFF;
        public readonly FormatHeader Format;
        public readonly DataHeader Data;

        public WAVHeader(RIFFHeader riff, FormatHeader format, DataHeader data)
        {
            RIFF = riff;
            Format = format;
            Data = data;
        }

        public static WAVHeader FromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var riff = RIFFHeader.FromStream(reader);
            var format = FormatHeader.FromStream(reader);
            var data = DataHeader.FromStream(reader);
            return new WAVHeader(riff, format, data);
        }

        public SampleType GetSampleType()
            => Format.Tag switch
            {
                WAVFormatTag.PCM when Format.BitsPerSample == 16 => SampleType.UInt16,
                WAVFormatTag.IEEEFloat when Format.BitsPerSample == 32 => SampleType.Float32,
                _ => SampleType.Unknown,
            };

        public SampleFormat GetSampleFormat()
            => new SampleFormat(GetSampleType(), (int)Format.SamplesPerSecond, Format.ChannelCount);

    }
}
