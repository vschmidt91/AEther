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

        protected readonly T Resource;

        readonly Lazy<RenderTargetView> RTView;
        readonly Lazy<DepthStencilView> DSView;
        readonly Lazy<ShaderResourceView> SRView;

        public DepthStencilView DepthStencilView => DSView.Value;
        public RenderTargetView RenderTargetView => RTView.Value;
        public ShaderResourceView ShaderResourceView => SRView.Value;

        public GraphicsResource(T resource)
        {
            Resource = resource;
            RTView = new Lazy<RenderTargetView>(() => new RenderTargetView(Resource.Device, Resource));
            DSView = new Lazy<DepthStencilView>(() => new DepthStencilView(Resource.Device, Resource));
            SRView = new Lazy<ShaderResourceView>(() => new ShaderResourceView(Resource.Device, Resource));
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
            if (RTView.IsValueCreated)
                RTView.Value.Dispose();
            if (DSView.IsValueCreated)
                DSView.Value.Dispose();
            if (SRView.IsValueCreated)
                SRView.Value.Dispose();
        }

    }
}
