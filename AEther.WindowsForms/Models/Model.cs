﻿using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace AEther.WindowsForms
{

    public class Model : IDisposable
    {

        public readonly Buffer VertexBuffer;
        public readonly VertexBufferBinding VertexBufferBinding;
        public readonly Buffer IndexBuffer;

        protected bool IsDisposed;

        public Model(SharpDX.Direct3D11.Device device, Mesh mesh)
        {
            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, mesh.Vertices);
            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, mesh.Indices);
            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, Marshal.SizeOf<Vertex>(), 0);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }
    }
}
