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

        public ShaderState(Graphics graphics, Shader shader)
            : base(graphics)
        {
            Shader = shader;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public override void Render()
        {
            Graphics.SetModel();
            Graphics.SetRenderTarget(null, Graphics.BackBuffer);
            Graphics.Draw(Shader);
        }

    }
}
