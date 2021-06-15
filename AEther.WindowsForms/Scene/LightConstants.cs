using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 208)]
    public struct LightConstants
    {

        [FieldOffset(0)]
        public Matrix View;

        [FieldOffset(64)]
        public Matrix Projection;

        [FieldOffset(128)]
        public Vector3 Intensity;

        [FieldOffset(140)]
        public float Anisotropy;

        [FieldOffset(144)]
        public Vector3 Position;

        [FieldOffset(156)]
        public float Distance;

        [FieldOffset(160)]
        public Vector3 Emission;

        [FieldOffset(172)]
        public float ShadowFarPlane;

        [FieldOffset(176)]
        public Vector3 Scattering;

        [FieldOffset(192)]
        public Vector3 Absorption;

    }
}
