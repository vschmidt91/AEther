using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct RuntimeConstants
    {

        [FieldOffset(0)]
        public Vector4 Dummy;


    }
}
