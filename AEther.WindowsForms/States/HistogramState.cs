namespace AEther.WindowsForms
{
    public class HistogramState : ShaderState
    {

        public HistogramState(Graphics graphics, Histogram[] histogram)
            : base(graphics, graphics.LoadShader("histogram.fx"))
        {
            for (var i = 0; i < histogram.Length; ++i)
            {
                Shader.Variables["Histogram" + i.ToString()].AsShaderResource().SetResource(histogram[i].Texture.SRView);
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
