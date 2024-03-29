﻿using System.IO;
using System.Text;

namespace AEther
{
    public record RIFFHeader
    (
        string RIFF,
        uint Length,
        string WAVE
    )
    {

        public static RIFFHeader FromStream(BinaryReader reader)
        {
            var riff = Encoding.UTF8.GetString(reader.ReadBytes(4));
            var length = reader.ReadUInt32();
            var wave = Encoding.UTF8.GetString(reader.ReadBytes(4));
            return new RIFFHeader(riff, length, wave);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Encoding.UTF8.GetBytes(RIFF));
            writer.Write(Length);
            writer.Write(Encoding.UTF8.GetBytes(WAVE));
        }

    }
}
