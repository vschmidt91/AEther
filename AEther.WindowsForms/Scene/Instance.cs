using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct Instance
    {

        [FieldOffset(0)]
        public Matrix4x4 Transform;

        [FieldOffset(64)]
        public Vector3 Color;

    }
}
