using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace AEther.WindowsForms
{
    public interface IModel
    {

        Buffer VertexBuffer { get; }
        VertexBufferBinding VertexBufferBinding { get; }
        Buffer IndexBuffer { get; }
        BoundingBox Bounds { get; }

    }
}
