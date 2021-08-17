using SharpDX;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 192)]
    public struct CameraConstants
    {

        [FieldOffset(0)]
        public Matrix View;

        [FieldOffset(64)]
        public Matrix Projection;

        [FieldOffset(128)]
        public Vector3 ViewPosition;

        [FieldOffset(140)]
        public float FarPlane;

        [FieldOffset(144)]
        public Matrix ViewDirectionMatrix;

    }
}
