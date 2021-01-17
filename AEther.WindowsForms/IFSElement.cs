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

        public IFSElement(Graphics graphics, string shader)
            : base(graphics)
        {
            Shader = Graphics.CreateShader(shader);
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
            Shader.ShaderResources["Source"].SetResource(source.GetShaderResourceView());
            Graphics.Draw(Shader);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            Shader.Dispose();
            WeightVariable.Dispose();
        }

    }
}
