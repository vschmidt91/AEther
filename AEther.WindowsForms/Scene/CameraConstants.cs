using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

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
