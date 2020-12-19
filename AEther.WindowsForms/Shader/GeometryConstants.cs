using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SharpDX;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct GeometryConstants
    {

        [FieldOffset(0)]
        public Matrix World;

        [FieldOffset(64)]
        public Vector3 Color;

    }
}
