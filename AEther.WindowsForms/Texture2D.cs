using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class Texture2D : GraphicsResource<SharpDX.Direct3D11.Texture2D>
    {

        public Texture2DDescription Description => Resource.Description;
        public int Width => Description.Width;
        public int Height => Description.Height;
        public Size Size => new(Width, Height);
        public Viewport ViewPort => new(0, 0, Width, Height, 0, 1);

        public Texture2D(SharpDX.Direct3D11.Texture2D texture)
            : base(texture)
        { }

        public void Clear(Color4 color = default)
        {
            Resource.Device.ImmediateContext.ClearRenderTargetView(RTView, color);
        }

    }
}
