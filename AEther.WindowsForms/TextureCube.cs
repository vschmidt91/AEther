using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public class TextureCube : Texture2D
    {

        public static readonly float FOV = MathUtil.DegreesToRadians(90);
        public const float NearPlane = .1f;
        public const float FarPlane = 100f;
        public static readonly Matrix Projection = Matrix.PerspectiveFovLH(FOV, 1, NearPlane, FarPlane);

        public static Matrix CreateView(int i, Vector3 position)
        {
            var direction = Vector3.Zero;
            var up = Vector3.Up;
            switch (i)
            {
                case 0: direction = Vector3.Right; break;
                case 1: direction = Vector3.Left; break;
                case 2: direction = Vector3.Up; up = Vector3.BackwardLH; break;
                case 3: direction = Vector3.Down; up = Vector3.ForwardLH; break;
                case 4: direction = Vector3.ForwardLH; break;
                case 5: direction = Vector3.BackwardLH; break;
            }
            return Matrix.LookAtLH(position, position + direction, up);
        }

        public readonly DepthStencilView[] DSViews;

        public TextureCube(Graphics graphics, int width, int height, Format format)
            : base(new SharpDX.Direct3D11.Texture2D(graphics.Device, new()
            {
                ArraySize = 6,
                BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Width = width,
                Height = height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            }))
        {
            var dsFormat = format switch
            {
                Format.R16_Typeless => Format.D16_UNorm,
                Format.R32_Typeless => Format.D32_Float,
                _ => throw new KeyNotFoundException()
            };
            DSViews = Enumerable.Range(0, Resource.Description.ArraySize)
                .Select(i => new DepthStencilView(Resource.Device, Resource, new()
                {
                    Format = dsFormat,
                    Dimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new()
                    {
                        ArraySize = 1,
                        FirstArraySlice = i,
                        MipSlice = 0,
                    },
                }))
                .ToArray();
        }

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
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube,
                TextureCube = new()
                {
                    MipLevels = -1,
                    MostDetailedMip = 0,
                },
            });
        }

    }
}
