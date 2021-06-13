using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class SpectrumState : ShaderState
    {

        public SpectrumState(Graphics graphics, SpectrumAccumulator[] spectrum)
            : base(graphics, graphics.CreateShader("spectrum.fx"))
        {
            for (var i = 0; i < spectrum.Length; ++i)
            {
                Shader.Variables["Spectrum" + i.ToString()].AsShaderResource().SetResource(spectrum[i].Texture.SRView);
            }
        }

        public override void Dispose()
        {
            Shader.Dispose();
            base.Dispose();
        }

    }
}
