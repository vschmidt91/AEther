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

        public override void Render()
        {

            Graphics.SetModel(null);
            Graphics.Context.Rasterizer.SetViewport(Graphics.BackBuffer.ViewPort);
            Graphics.Context.OutputMerger.SetRenderTargets(null, Graphics.BackBuffer.GetRenderTargetView());
            Graphics.Draw(Shader);

        }

        public override string ToString()
        {
            return $"{ GetType().Name } { Shader.Name }";
        }

    }
}
