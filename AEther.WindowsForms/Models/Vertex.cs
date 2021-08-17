using SharpDX;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct Vertex
    {

        [FieldOffset(0)]
        public Vector3 Position;

        [FieldOffset(16)]
        public Vector3 Normal;

        [FieldOffset(32)]
        public Vector2 UV;

    }
}
