using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

namespace AEther
{
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct RowMatrix
    {

        [FieldOffset(0)]
        public Matrix4x4 Matrix;

        [FieldOffset(0)]
        public Vector4 Row0;

        [FieldOffset(16)]
        public Vector4 Row1;

        [FieldOffset(32)]
        public Vector4 Row2;

        [FieldOffset(48)]
        public Vector4 Row3;

        public RowMatrix(Matrix4x4 matrix)
        {
            Row0 = Row1 = Row2 = Row3 = Vector4.Zero;
            Matrix = matrix;
        }

        public RowMatrix(Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
        {
            Matrix = new Matrix4x4();
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        public Vector4 this[int i]
        {
            get
            {
                switch(i)
                {
                    case 0: return Row0;
                    case 1: return Row1;
                    case 2: return Row2;
                    case 3: return Row3;
                    default: return default;
                }
            }
            set
            {
                switch (i)
                {
                    case 0: Row0 = value; break;
                    case 1: Row1 = value; break;
                    case 2: Row2 = value; break;
                    case 3: Row3 = value; break;
                }
            }
        }

    }
}
