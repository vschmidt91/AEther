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

        public RenderTargetView RTView => RTViewLazy.Value;
        public DepthStencilView DSView => DSViewLazy.Value;
        public ShaderResourceView SRView => SRViewLazy.Value;

        readonly Lazy<RenderTargetView> RTViewLazy;
        readonly Lazy<DepthStencilView> DSViewLazy;
        readonly Lazy<ShaderResourceView> SRViewLazy;

        protected bool IsDisposed;

        public GraphicsResource(T resource)
        {
            Resource = resource;
            RTViewLazy = new Lazy<RenderTargetView>(() => new RenderTargetView(Resource.Device, Resource));
            DSViewLazy = new Lazy<DepthStencilView>(() => new DepthStencilView(Resource.Device, Resource));
            SRViewLazy = new Lazy<ShaderResourceView>(() => new ShaderResourceView(Resource.Device, Resource));
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
            if(!IsDisposed)
            {
                if (RTViewLazy.IsValueCreated)
                    RTViewLazy.Value.Dispose();
                if (DSViewLazy.IsValueCreated)
                    DSViewLazy.Value.Dispose();
                if (SRViewLazy.IsValueCreated)
                    SRViewLazy.Value.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
