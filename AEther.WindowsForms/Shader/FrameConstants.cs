using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct FrameConstants
    {

        [FieldOffset(0)]
        public Vector4 Time;

        [FieldOffset(16)]
        public float AspectRatio;


    }
}
