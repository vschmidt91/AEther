using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using Vector4 = System.Numerics.Vector4;

namespace AEther.WindowsForms
{
    public class IFSElement : IDisposable
    {

        public readonly EffectVectorVariable WeightVariable;
        public readonly Shader Shader;

        public Vector4 Weight { get; set; } = Vector4.One;
        public float Speed { get; set; } = .1f;

        public IFSElement(Shader shader)
        {
            Shader = shader;
            WeightVariable = Shader.Variables["Weight"].AsVector();
            WeightVariable.GetVector<Vector4>();
        }

        public virtual void Update(float t)
        {
            //Weight = Enumerable.Range(0, 4)
            //    .Select(k => 1 + Math.Sin(Speed * t + k))
            //    .Select(x => (float)x)
            //    .ToArray()
            //    .ToVector4();
        }

        public virtual void Dispose()
        {
            WeightVariable.Dispose();
        }

    }
}
