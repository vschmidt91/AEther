using System;

namespace AEther.WindowsForms
{
    public class SpectrumState : ShaderState
    {

        public SpectrumState(Graphics graphics, SpectrumAccumulator[] spectrum)
            : base(graphics, graphics.LoadShader("spectrum.fx"))
        {
            for (var i = 0; i < spectrum.Length; ++i)
            {
                Shader.Variables["Spectrum" + i.ToString()].AsShaderResource().SetResource(spectrum[i].Texture.SRView);
            }
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Shader.Dispose();
            base.Dispose();
        }

    }
}
