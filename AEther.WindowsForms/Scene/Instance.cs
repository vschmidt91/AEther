using SharpDX;
using System.Runtime.InteropServices;

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
