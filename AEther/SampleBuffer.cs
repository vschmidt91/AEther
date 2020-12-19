using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    [StructLayout(LayoutKind.Explicit)]
    public class SampleBuffer
    {

        [FieldOffset(0)]
        readonly byte[] Bytes;

        [FieldOffset(0)]
        readonly float[] Floats;

        [FieldOffset(0)]
        readonly ushort[] UShorts;

        public int ByteCount => Bytes.Length;
        public int FloatCount => ByteCount / sizeof(float);

        public float this[int i] => Floats[i];
        public Span<byte> Span => Bytes;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public SampleBuffer(int size)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            Bytes = new byte[size];
        }

        public static SampleBuffer Create<T>(int size)
        {
            return new SampleBuffer(size * Marshal.SizeOf<T>());
        }

        public float GetFloat(int i)
        {
            return Floats[i];
        }

        public float GetUShort(int i)
        {
            return UShorts[i] / 32768f - 1;
        }

        public int GetSize<T>()
        {
            return Bytes.Length / Marshal.SizeOf<T>();
        }

    }

}
