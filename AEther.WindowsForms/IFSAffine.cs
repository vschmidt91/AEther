using SharpDX.Direct3D11;
using System.Numerics;

namespace AEther.WindowsForms
{
    public class IFSAffine : IFSElement
    {

        public Vector4 Transform
        {
            get => TransformVariable.GetFloatVector().ToVector4();
            set => TransformVariable.Set(value);
        }

        public Vector2 Offset
        {
            get => OffsetVariable.GetFloatVector().ToVector4().XY();
            set => OffsetVariable.Set(value);
        }

        readonly EffectVectorVariable TransformVariable;
        readonly EffectVectorVariable OffsetVariable;

        public float Scale;
        public float OffsetScale;

        public IFSAffine(Graphics graphics)
            : base(graphics, "ifs-affine.fx")
        {
            TransformVariable = Shader.Variables["Transform"].AsVector();
            OffsetVariable = Shader.Variables["Offset"].AsVector();
            Scale = 1f;
            OffsetScale = 1f;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            TransformVariable.Dispose();
            OffsetVariable.Dispose();
            base.Dispose();
        }

        public override void Update(float t)
        {
            base.Update(t);
            float a = Speed * t;
            Transform = Scale * new Vector4
            {
                X = (float)+Math.Cos(a),
                Y = (float)-Math.Sin(a),
                Z = (float)+Math.Sin(a),
                W = (float)+Math.Cos(a),
            };
            float b = 1.4756346f * Speed * t;
            Offset = OffsetScale * new Vector2
            {
                X = (float)Math.Cos(b),
                Y = (float)Math.Sin(b),
            };

            //Transform.Set(new[] { 1, .5f, -.5f, 1 });
            //Offset.Set(new[] { 1f, -.3f });

        }

    }
}
