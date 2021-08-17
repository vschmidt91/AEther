
using SharpDX;

namespace AEther.WindowsForms
{
    public class CameraOrthographic : Camera
    {

        public float Width { get; set; } = 1024;
        public float Height { get; set; } = 1024;

        public override Matrix Projection => Matrix.OrthoLH(Width, Height, NearPlane, FarPlane);

    }
}
