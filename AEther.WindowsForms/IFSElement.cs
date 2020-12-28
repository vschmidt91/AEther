using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AEther.WindowsForms
{
    public class IFSElement : IDisposable
    {

        public Shader Shader;

        public Vector4 Weight { get; set; }
        public float Speed { get; set; }

        public IFSElement(Shader shader)
        {
            Shader = shader;
            Weight = Vector4.One;
            Speed = 1f;
        }

        public virtual void Update(float t)
        {
            Weight = Enumerable.Range(0, 4)
                .Select(k => 1 + Math.Sin(Speed * t + k))
                .Select(x => (float)x)
                .ToArray()
                .ToVector4();
        }

        public virtual void Dispose()
        {
            SharpDX.Utilities.Dispose(ref Shader);
        }

    }
}
