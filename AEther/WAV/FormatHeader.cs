using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;

namespace AEther
{
    public readonly struct FormatHeader
    {

        public readonly string FMT;
        public readonly uint Length;
        public readonly WAVFormatTag Tag;
        public readonly ushort ChannelCount;
        public readonly uint SamplesPerSecond;
        public readonly uint BytesPerSecond;
        public readonly ushort BlockLength;
        public readonly ushort BitsPerSample;

        public FormatHeader(string fmt, uint length, WAVFormatTag tag, ushort channelCount, uint samplesPerSecond, ushort bitsPerSample, uint? bytesPerSecond = default, ushort? blockLength = default)
        {
            FMT = fmt;
            Length = length;
            Tag = tag;
            ChannelCount = channelCount;
            SamplesPerSecond = samplesPerSecond;
            BitsPerSample = bitsPerSample;
            BlockLength = blockLength ?? (ushort)((BitsPerSample + 7) / 8 * ChannelCount);
            BytesPerSecond = bytesPerSecond ?? (SamplesPerSecond * BlockLength);
        }

        public static FormatHeader FromStream(BinaryReader reader)
        {
            var fmt = Encoding.UTF8.GetString(reader.ReadBytes(4));
            var length = reader.ReadUInt32();
            var tag = (WAVFormatTag)reader.ReadUInt16();
            var channelCount = reader.ReadUInt16();
            var samplesPerSecond = reader.ReadUInt32();
            var bytesPerSecond = reader.ReadUInt32();
            var blockLength = reader.ReadUInt16();
            var bitsPerSample = reader.ReadUInt16();
            return new FormatHeader(fmt, length, tag, channelCount, samplesPerSecond, bitsPerSample, bytesPerSecond, blockLength);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Encoding.UTF8.GetBytes(FMT));
            writer.Write(Length);
            writer.Write((ushort)Tag);
            writer.Write(ChannelCount);
            writer.Write(SamplesPerSecond);
            writer.Write(BytesPerSecond);
            writer.Write(BlockLength);
            writer.Write(BitsPerSample);
        }

    }
}
