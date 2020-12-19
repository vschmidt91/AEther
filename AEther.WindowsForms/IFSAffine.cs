using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace AEther.WindowsForms
{
    public class IFSAffine : IFSElement
    {

        public EffectVectorVariable Transform;
        public EffectVectorVariable Offset;

        public float Speed;
        public float Scale;
        public float OffsetScale;

        public IFSAffine(ShaderManager shader)
            : this(shader["ifs-affine.fx"])
        {
        }

        public IFSAffine(Shader shader)
            : base(shader)
        {
            Transform = Shader.Variables["Transform"].AsVector();
            Offset = Shader.Variables["Offset"].AsVector();
            Scale = 1f;
            OffsetScale = 1f;
        }

        public override void Dispose()
        {
            Utilities.Dispose(ref Transform);
            Utilities.Dispose(ref Offset);
            base.Dispose();
        }

        public override void Update(float t)
        {
            float a = Speed * t;
            Transform.Set(new[]
            {
                +Scale * (float)Math.Cos(a),
                -Scale * (float)Math.Sin(a),
                +Scale * (float)Math.Sin(a),
                +Scale * (float)Math.Cos(a),
            });
            float b = 1.4756346f * Speed * t;
            Offset.Set(new[]
            {
                OffsetScale * (float)Math.Cos(b),
                OffsetScale * (float)Math.Sin(b)
            });
        }

    }
}
