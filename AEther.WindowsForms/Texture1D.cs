using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class Texture1D : GraphicsResource<SharpDX.Direct3D11.Texture1D>
    {

        public Texture1DDescription Description => Resource.Description;
        public int Size => Description.Width;

        public Texture1D(SharpDX.Direct3D11.Texture1D texture)
            : base(texture)
        { }

        public void Clear(DeviceContext context = default, Vector4? color = default)
        {
            var ctx = context ?? Resource.Device.ImmediateContext;
            ctx.ClearRenderTargetView(GetRenderTargetView(), new Color4(color ?? Vector4.Zero));
        }

    }
}
