using SharpDX.Direct3D11;
using System;
using Vector4 = System.Numerics.Vector4;

namespace AEther.WindowsForms
{
    public class IFSElement : GraphicsComponent, IDisposable
    {

        public Vector4 Weight
        {
            get => WeightVariable.GetFloatVector().ToVector4();
            set => WeightVariable.Set(value);
        }

        readonly EffectVectorVariable WeightVariable;
        protected readonly Shader Shader;

        public float Speed { get; set; } = .1f;
        protected bool IsDisposed;

        public IFSElement(Graphics graphics, string shader)
            : base(graphics)
        {
            Shader = Graphics.LoadShader(shader);
            WeightVariable = Shader.Variables["Weight"].AsVector();
            Weight = Vector4.One;
        }

        public virtual void Update(float t)
        {
            //Weight = Enumerable.Range(0, 4)
            //    .Select(k => 1 + Math.Sin(Speed * t + k))
            //    .Select(x => (float)x)
            //    .ToArray()
            //    .ToVector4();
        }

        public virtual void Draw(Texture2D source)
        {
            Shader.ShaderResources["Source"].SetResource(source.SRView);
            Graphics.Draw(Shader);
        }

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                WeightVariable.Dispose();
                Shader.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
