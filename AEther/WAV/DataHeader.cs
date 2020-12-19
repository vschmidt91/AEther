using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace AEther
{
    public readonly struct DataHeader
    {

        public readonly string Signature;
        public readonly uint Length;

        public DataHeader(string signature, uint length)
        {
            Signature = signature;
            Length = length;
        }

        public static DataHeader FromStream(BinaryReader reader)
        {
            var signature = Encoding.UTF8.GetString(reader.ReadBytes(4));
            var length = reader.ReadUInt32();
            return new DataHeader(signature, length);
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(Encoding.UTF8.GetBytes(Signature));
            writer.Write(Length);
        }

    }
}
