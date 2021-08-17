using SharpDX.DXGI;

namespace AEther.WindowsForms
{
    public class GeometryBuffer : IDisposable
    {

        public readonly DepthBuffer Depth;
        public readonly Texture2D Normal;
        public readonly Texture2D Color;

        public readonly Texture2D[] Textures;

        public GeometryBuffer(Graphics graphics, int width, int height)
        {
            Depth = graphics.CreateDepthBuffer(width, height, Format.R16_Typeless);
            Normal = graphics.CreateTexture(width, height, Format.R8G8B8A8_SNorm);
            Color = graphics.CreateTexture(width, height, Format.R8G8B8A8_UNorm);
            Textures = new[] { Normal, Color };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Depth.Dispose();
            Normal.Dispose();
            Depth.Dispose();
        }
    }
}
