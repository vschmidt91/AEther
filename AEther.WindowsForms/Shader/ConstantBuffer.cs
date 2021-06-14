using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace AEther.WindowsForms
{
    public class ConstantBuffer<T> : IDisposable
        where T : struct
    {

        public readonly Buffer Buffer;

        public T Value = default;
        public bool UseResourceMapping = true;

        protected bool IsDisposed;
        
        public ConstantBuffer(Device device, BufferDescription? description = default)
        {

            Buffer = new Buffer(device, description ?? new BufferDescription()
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Marshal.SizeOf(Value),
                StructureByteStride = 0,
                Usage = ResourceUsage.Dynamic,
            });

        }

        public void Update()
        {
            var context = Buffer.Device.ImmediateContext;
            if (UseResourceMapping)
            {
                context.MapSubresource(Buffer, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                stream.Write(Value);
                context.UnmapSubresource(Buffer, 0);
            }
            else
            {
                context.UpdateSubresource(ref Value, Buffer);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Buffer.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
