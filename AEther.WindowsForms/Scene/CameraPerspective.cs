using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class CameraPerspective : Camera
    {

        public float FieldOfView { get; set; } = 75;
        public float AspectRatio { get; set; } = 16 / 9f;
        
        public override Matrix4x4 Projection => Matrix4x4.CreatePerspectiveFieldOfView(SharpDX.MathUtil.DegreesToRadians(FieldOfView), AspectRatio, NearPlane, FarPlane);

    }
}
