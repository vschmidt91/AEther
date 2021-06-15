using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class DepthBuffer : Texture2D
    {

        public DepthBuffer(SharpDX.Direct3D11.Texture2D texture)
            : base(texture)
        { }

        protected override ShaderResourceView CreateSRView()
        {
            var format = Resource.Description.Format switch
            {
                Format.R16_Typeless => Format.R16_UNorm,
                Format.R32_Typeless => Format.R32_Float,
                _ => throw new KeyNotFoundException()
            };
            return new ShaderResourceView(Resource.Device, Resource, new()
            {
                Format = format,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new()
                {
                    MipLevels = -1,
                    MostDetailedMip = 0,
                },
            });
        }

        protected override DepthStencilView CreateDSView()
        {
            var format = Resource.Description.Format switch
            {
                Format.R16_Typeless => Format.D16_UNorm,
                Format.R32_Typeless => Format.D32_Float,
                _ => throw new KeyNotFoundException()
            };
            return new DepthStencilView(Resource.Device, Resource, new()
            {
                Format = format,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new()
                {
                    MipSlice = 0,
                },
            });
        }

    }
}
