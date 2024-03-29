﻿
using SharpDX;

namespace AEther.WindowsForms
{
    public abstract class Camera
    {

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Direction { get; set; } = Vector3.ForwardLH;

        public Matrix View => Matrix.LookAtLH(Position, Position + Direction, Vector3.Up);

        public float NearPlane { get; set; } = 1;
        public float FarPlane { get; set; } = 100;

        public abstract Matrix Projection { get; }

        static readonly int[] Idx = new[] { 0, 1 };
        public Vector3[,,] GetBounds()
        {
            var invViewProjection = Matrix.Invert(Matrix.Multiply(View, Projection));
            var bounds = new Vector3[2, 2, 2];
            foreach (int x in Idx)
            {
                foreach (int y in Idx)
                {
                    foreach (int z in Idx)
                    {
                        var clipPos = new Vector4(-1 + 2 * x, -1 + 2 * y, z, 1);
                        var worldPos = Vector4.Transform(clipPos, invViewProjection);
                        bounds[x, y, z] = new Vector3
                        {
                            X = worldPos.X,
                            Y = worldPos.Y,
                            Z = worldPos.Z,
                        } / worldPos.W;
                    }
                }
            }
            return bounds;
        }

        public Matrix GetViewDirectionMatrix()
        {
            var bounds = GetBounds();
            var topLeft = bounds[0, 1, 1];
            var farPosMatrix = Matrix.Identity;
            farPosMatrix.Column1 = new Vector4(bounds[1, 1, 1] - topLeft, 0);
            farPosMatrix.Column2 = new Vector4(bounds[0, 0, 1] - topLeft, 0);
            farPosMatrix.Column3 = new Vector4(topLeft - Position, 0);
            return farPosMatrix;
        }

    }
}
