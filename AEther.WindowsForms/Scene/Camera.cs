using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public abstract class Camera
    {

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Vector3 Direction => Vector3.Normalize(Target - Position);
        public Matrix View => Matrix.LookAtLH(Position, Target, Vector3.Up);

        public float NearPlane { get; set; } = 1f;
        public float FarPlane { get; set; } = 1000;

        public abstract Matrix Projection { get; }

    }
}
