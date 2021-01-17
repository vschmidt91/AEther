using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class GraphicsResource<T> : IDisposable
        where T : Resource
    {

        public string DebugName
        {
            get => Resource.DebugName;
            set => Resource.DebugName = value;
        }

        protected T Resource;

        RenderTargetView? RTView;
        ShaderResourceView? SRView;
        DepthStencilView? DSView;

        public GraphicsResource(T resource)
        {
            Resource = resource;
        }

        public RenderTargetView CreateRenderTargetView(RenderTargetViewDescription? description = default)
        {
            if (description.HasValue)
            {
                return new RenderTargetView(Resource.Device, Resource, description.Value);
            }
            else
            {
                return new RenderTargetView(Resource.Device, Resource);
            }
        }

        public ShaderResourceView CreateShaderResourceView(ShaderResourceViewDescription? description = default)
        {
            if (description.HasValue)
            {
                return new ShaderResourceView(Resource.Device, Resource, description.Value);
            }
            else
            {
                return new ShaderResourceView(Resource.Device, Resource);
            }
        }

        public DepthStencilView CreateDdepthStencilView(DepthStencilViewDescription? description = default)
        {
            if (description.HasValue)
            {
                return new DepthStencilView(Resource.Device, Resource, description.Value);
            }
            else
            {
                return new DepthStencilView(Resource.Device, Resource);
            }
        }

        public RenderTargetView GetRenderTargetView()
        {
            if (RTView == null)
            {
                RTView = CreateRenderTargetView();
            }
            return RTView;
        }

        public DepthStencilView GetDepthStencilView()
        {
            if(DSView == null)
            {
                DSView = CreateDdepthStencilView();
            }
            return DSView;
        }

        public ShaderResourceView GetShaderResourceView()
        {
            if (SRView == null)
            {
                SRView = CreateShaderResourceView();
            }
            return SRView;
        }

        public ContextMapping Map(int? subResource = default, MapMode? mode = default, MapFlags? flags = default)
        {
            return new ContextMapping(Resource, subResource, mode, flags);
        }

        public void Update<S>(S[] data, int subResource = 0, ResourceRegion? region = null)
            where S : struct
        {
            Resource.Device.ImmediateContext.UpdateSubresource(data, Resource, subResource, 0, 0, region);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Utilities.Dispose(ref RTView);
            Utilities.Dispose(ref SRView);
            Utilities.Dispose(ref DSView);
            Utilities.Dispose(ref Resource);
        }

    }
}
