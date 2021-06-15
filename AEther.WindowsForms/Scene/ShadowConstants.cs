using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct ShadowConstants
    {

        [FieldOffset(0)]
        public Matrix View;

        [FieldOffset(64)]
        public Matrix Projection;

    }
}
