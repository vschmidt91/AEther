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

        public Buffer VertexBuffer;
        public VertexBufferBinding VertexBufferBinding;
        public Buffer IndexBuffer;
        public BoundingBox Bounds;

        readonly Mesh Mesh;

        public Model(SharpDX.Direct3D11.Device device, Mesh mesh)
        {

            Mesh = mesh;

            var min = Mesh.Vertices.Aggregate(+1e6f * Vector3.One, (acc, v) => Vector3.Min(acc, v.Position));
            var max = Mesh.Vertices.Aggregate(-1e6f * Vector3.One, (acc, v) => Vector3.Max(acc, v.Position));
            Bounds = new BoundingBox()
            {
                Minimum = min,
                Maximum = max,
            };

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Mesh.Vertices);
            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, Mesh.Indices);
            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, Marshal.SizeOf<Vertex>(), 0);

        }

        public void Dispose()
        {
            Utilities.Dispose(ref VertexBuffer);
            Utilities.Dispose(ref IndexBuffer);
        }
    }
}
