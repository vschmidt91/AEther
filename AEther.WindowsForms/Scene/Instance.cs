using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SharpDX;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 96)]
    public struct Instance
    {

        [FieldOffset(0)]
        public Matrix Transform;

        [FieldOffset(64)]
        public Vector4 Color;

        [FieldOffset(80)]
        public float Roughness;

    }
}
