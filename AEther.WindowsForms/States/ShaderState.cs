using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{

    public class ShaderState : GraphicsState
    {

        protected readonly Shader Shader;
        protected readonly string Name;

        public ShaderState(Graphics graphics, Shader shader, string name)
            : base(graphics)
        {
            Shader = shader;
            Name = name;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public override void Render()
        {
            Graphics.SetFullscreenTarget(Graphics.BackBuffer);
            Graphics.Draw(Shader);
        }

        public override string ToString()
        {
            return $"{ GetType().Name } { Name }";
        }

    }
}
