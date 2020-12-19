using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AEther
{
    public struct Eigencomposition
    {

        public readonly Vector4 Eigenvalues;
        public readonly RowMatrix Eigenvectors;

        public Eigencomposition(Vector4 eigenvalues, Vector4 eigenvector0, Vector4 eigenvector1, Vector4 eigenvector2, Vector4 eigenvector3)
        {
            Eigenvalues = eigenvalues;
            Eigenvectors = new RowMatrix(
                Vector4.Normalize(eigenvector0),
                Vector4.Normalize(eigenvector1),
                Vector4.Normalize(eigenvector2),
                Vector4.Normalize(eigenvector3));
        }

        public Matrix4x4 ToMatrix()
        {
            var q = Matrix4x4.Transpose(Eigenvectors.Matrix);
            Matrix4x4.Invert(q, out var qInv);
            var s = Eigenvalues.ToDiagonalMatrix();
            return q * s * qInv;
        }
        
    }
}
