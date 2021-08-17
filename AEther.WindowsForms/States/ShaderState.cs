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
