using SharpDX;
using System.Runtime.InteropServices;

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
        public float FarPlane;

        [FieldOffset(176)]
        public Vector3 Scattering;

        [FieldOffset(192)]
        public Vector3 Absorption;

    }
}
