using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class IFSAffine : IFSElement
    {

        public EffectVectorVariable Transform;
        public EffectVectorVariable Offset;

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
            Transform.Dispose();
            Offset.Dispose();
            base.Dispose();
        }

        public override void Update(float t)
        {
            base.Update(t);
            float a = Speed * t;
            Transform.Set(Scale * new Vector4
            {
                X = (float)+Math.Cos(a),
                Y = (float)-Math.Sin(a),
                Z = (float)+Math.Sin(a),
                W = (float)+Math.Cos(a),
            });
            float b = 1.4756346f * Speed * t;
            Offset.Set(OffsetScale * new Vector2
            {
                X = (float)Math.Cos(b),
                Y = (float)Math.Sin(b),
            });

            //Transform.Set(new[] { 1, .5f, -.5f, 1 });
            //Offset.Set(new[] { 1f, -.3f });

        }

    }
}
