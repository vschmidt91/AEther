using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class CameraOrthographic : Camera
    {

        public float Width { get; set; } = 1024;
        public float Height { get; set; } = 1024;

        public override Matrix4x4 Projection => Matrix4x4.CreateOrthographic(Width, Height, NearPlane, FarPlane);

    }
}
