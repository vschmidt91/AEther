using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public abstract class Camera
    {

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Vector3 Direction => Vector3.Normalize(Target - Position);
        public Matrix4x4 View => Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);

        public float NearPlane { get; set; } = 1f;
        public float FarPlane { get; set; } = 1000;

        public abstract Matrix4x4 Projection { get; }

        static readonly int[] Idx = new[] { 0, 1 };
        public Vector3[,,] GetBounds()
        {
            Matrix4x4.Invert(View * Projection, out var invViewProjection);
            Vector3[,,] bounds = new Vector3[2, 2, 2];
            foreach(int x in Idx)
            {
                foreach (int y in Idx)
                {
                    foreach (int z in Idx)
                    {
                        Vector4 clipPos = new Vector4(-1 + 2 * x, -1 + 2 * y, z, 1);
                        Vector4 worldPos = Vector4.Transform(clipPos, invViewProjection);
                        bounds[x, y, z] = worldPos.XYZ() / worldPos.W;
                    }
                }
            }
            return bounds;
        }

    }
}
