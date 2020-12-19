using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public class Particles : ComputeBuffer
    {

        public Vector4 Attractor = new Vector4(0, 0, 0, 1);
        public Vector3 Color = Vector3.One;
        public float Emission = 1;

        public Particles(SharpDX.Direct3D11.Device device, int count)
            : base(device, 64, count, false)
        { }

    }
}
