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
    public class FloatBuffer
    {

        [FieldOffset(0)]
        public readonly byte[] Bytes;

        [FieldOffset(0)]
        public readonly float[] Floats;

        public int ByteCount => Bytes.Length;
        public int FloatCount => ByteCount / sizeof(float);

        public FloatBuffer(int size)
        {
            Bytes = new byte[sizeof(float) * size];
        }

    }
}
