using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AEther.WindowsForms
{
    public class IFSElement : IDisposable
    {

        public Shader Shader;

        public Vector4 Weight { get; set; }

        public IFSElement(Shader shader)
        {
            Shader = shader;
        }

        public virtual void Update(float t)
        {

        }

        public virtual void Dispose()
        {
            SharpDX.Utilities.Dispose(ref Shader);
        }

    }
}
