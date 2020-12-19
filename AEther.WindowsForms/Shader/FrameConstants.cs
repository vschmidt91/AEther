using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 160)]
    public struct FrameConstants
    {

        [FieldOffset(0)]
        public Matrix4x4 View;

        [FieldOffset(64)]
        public Matrix4x4 Projection;

        [FieldOffset(128)]
        public Vector4 Time;

        [FieldOffset(128 + 16)]
        public float HistogramShift;

        [FieldOffset(128 + 20)]
        public float AspectRatio;


    }
}
