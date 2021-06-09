using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace AEther.WindowsForms
{

    public class Model : IDisposable
    {

        public readonly Buffer VertexBuffer;
        public readonly VertexBufferBinding VertexBufferBinding;
        public readonly Buffer IndexBuffer;

        public Model(SharpDX.Direct3D11.Device device, Mesh mesh)
        {
            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, mesh.Vertices);
            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, mesh.Indices);
            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, Marshal.SizeOf<Vertex>(), 0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
