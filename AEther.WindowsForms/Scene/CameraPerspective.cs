
using SharpDX;

namespace AEther.WindowsForms
{
    public class CameraPerspective : Camera
    {

        public float FieldOfView { get; set; } = MathUtil.DegreesToRadians(75);
        public float AspectRatio { get; set; } = 16 / 9f;

        public override Matrix Projection => Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, NearPlane, FarPlane);

    }
}
