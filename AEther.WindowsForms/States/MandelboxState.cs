using System;

namespace AEther.WindowsForms
{
    public class MandelboxState : ShaderState
    {

        public MandelboxState(Graphics graphics, SpectrumAccumulator[] spectrum)
            : base(graphics, graphics.LoadShader("mandelbox.fx"))
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
