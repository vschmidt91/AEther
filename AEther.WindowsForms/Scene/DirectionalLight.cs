using SharpDX;

namespace AEther.WindowsForms
{
    public class DirectionalLight : Light
    {

        public Vector3 Direction => Vector3.Normalize(Transform.Translation);

    }
}
