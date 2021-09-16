using System.IO;
using System.Text;

namespace AEther
{
    public record DataHeader
    (
        string Signature,
        uint Length
    )
    {

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
