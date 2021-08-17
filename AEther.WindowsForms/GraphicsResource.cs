
using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class GraphicsResource<T> : IDisposable
        where T : Resource
    {

        public RenderTargetView RTView => RTViewLazy.Value;
        public DepthStencilView DSView => DSViewLazy.Value;
        public ShaderResourceView SRView => SRViewLazy.Value;

        protected readonly T Resource;
        protected readonly Lazy<RenderTargetView> RTViewLazy;
        protected readonly Lazy<DepthStencilView> DSViewLazy;
        protected readonly Lazy<ShaderResourceView> SRViewLazy;

        protected bool IsDisposed;

        public GraphicsResource(T resource)
        {
            Resource = resource;
            RTViewLazy = new Lazy<RenderTargetView>(CreateRTView);
            DSViewLazy = new Lazy<DepthStencilView>(CreateDSView);
            SRViewLazy = new Lazy<ShaderResourceView>(CreateSRView);
        }

        protected virtual RenderTargetView CreateRTView() => new(Resource.Device, Resource);
        protected virtual DepthStencilView CreateDSView() => new(Resource.Device, Resource);
        protected virtual ShaderResourceView CreateSRView() => new(Resource.Device, Resource);

        public ContextMapping Map()
        {
            return new ContextMapping(Resource);
        }

        public void Update<S>(S[] data, int subResource = 0, ResourceRegion? region = null)
            where S : struct
        {
            Resource.Device.ImmediateContext.UpdateSubresource(data, Resource, subResource, 0, 0, region);
        }

        public virtual void Clear(Color4 color = default)
        {
            Resource.Device.ImmediateContext.ClearRenderTargetView(RTView, color);
        }

        public virtual void ClearDepth(float depth = 1f)
        {
            Resource.Device.ImmediateContext.ClearDepthStencilView(DSView, DepthStencilClearFlags.Depth, depth, 0);
        }

        public void Dispose()
        {
            if (!IsDisposed)
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
