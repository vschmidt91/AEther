using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

using Device = SharpDX.Direct3D11.Device;

namespace AEther.WindowsForms
{
    public class ComputeBuffer : IDisposable
    {

        public int Size => Description.SizeInBytes / Description.StructureByteStride;

        public BufferDescription Description => Buffer.Description;

        readonly Buffer Buffer;
        UnorderedAccessView? UAView;
        ShaderResourceView? SRView;

        public ComputeBuffer(Device device, int stride, int size, bool cpuWrite)
        {
            Buffer = new Buffer(device, new BufferDescription()
            {
                BindFlags = cpuWrite ? BindFlags.ShaderResource : BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                CpuAccessFlags = cpuWrite ? CpuAccessFlags.Write : CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = stride * size,
                StructureByteStride = stride,
                Usage = cpuWrite ? ResourceUsage.Dynamic : ResourceUsage.Default,
            });
        }

        public UnorderedAccessView GetUnorderedAccessView(UnorderedAccessViewDescription? description = default)
        {
            if(UAView == null)
            {
                UAView = new UnorderedAccessView(Buffer.Device, Buffer, description ?? new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        ElementCount = Size,
                        FirstElement = 0,
                        Flags = UnorderedAccessViewBufferFlags.None
                    },
                    Format = Format.Unknown,
                });
            }
            return UAView;
        }

        public ShaderResourceView GetShaderResourceView(ShaderResourceViewDescription? description = default)
        {
            if (SRView == null)
            {
                SRView = new ShaderResourceView(Buffer.Device, Buffer, description ?? new ShaderResourceViewDescription
                {
                    Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                    BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource()
                    {
                        ElementCount = Size,
                        FirstElement = 0,
                        Flags = ShaderResourceViewExtendedBufferFlags.None,
                    },
                    Format = Format.Unknown,
                });
            }
            return SRView;
        }

        public void Update<T>(DeviceContext context, ArraySegment<T> values)
            where T : struct
        {

            context.MapSubresource(
                Buffer,
                MapMode.WriteDiscard,
                SharpDX.Direct3D11.MapFlags.None,
                out DataStream stream);

            stream.WriteRange(values.Array, values.Offset, values.Count);

            context.UnmapSubresource(Buffer, 0);

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            UAView?.Dispose();
            SRView?.Dispose();
            Buffer.Dispose();
        }

    }
}
