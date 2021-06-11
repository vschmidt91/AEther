using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class HistogramState : ShaderState
    {

        public HistogramState(Graphics graphics, Histogram[] histogram)
            : base(graphics, graphics.CreateShader("histogram.fx"), "Histogram")
        {
            for(var i = 0; i < histogram.Length; ++i)
            {
                Shader.Variables["Histogram" + i.ToString()].AsShaderResource().SetResource(histogram[i].Texture.SRView);
            }
        }

        public override void Dispose()
        {
            Shader.Dispose();
            base.Dispose();
        }

    }
}
