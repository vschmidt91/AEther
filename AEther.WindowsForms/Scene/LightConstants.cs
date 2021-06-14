using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 144)]
    public struct LightConstants
    {

        [FieldOffset(0)]
        public Matrix Transform;

        [FieldOffset(64)]
        public Vector3 Intensity;

        [FieldOffset(76)]
        public float Anisotropy;

        [FieldOffset(80)]
        public Vector3 PositionOrDirection;

        [FieldOffset(92)]
        public float Distance;

        [FieldOffset(96)]
        public Vector3 Emission;

        [FieldOffset(112)]
        public Vector3 Scattering;

        [FieldOffset(128)]
        public Vector3 Absorption;

    }
}
