using System.IO;
using System.Text;

namespace AEther
{
    public record FormatHeader
    (
        string FMT,
        uint Length,
        WAVFormatTag Tag,
        ushort ChannelCount,
        uint SamplesPerSecond,
        uint BytesPerSecond,
        ushort BlockLength,
        ushort BitsPerSample
    )
    {

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
            return new FormatHeader(fmt, length, tag, channelCount, samplesPerSecond, bytesPerSecond, blockLength, bitsPerSample);
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
