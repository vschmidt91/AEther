using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class Particles : GraphicsComponent, IDisposable
    {

        readonly ComputeBuffer Buffer;
        readonly Shader ShaderSimulate;
        readonly Shader ShaderDraw;
        readonly Model Model;

        public Particles(Graphics graphics, int count, ConstantBuffer<CameraConstants> cameraConstants)
            : base(graphics)
        {

            Buffer = new ComputeBuffer(Graphics.Device, 80, count, false);
            ShaderSimulate = Graphics.CreateShader("particles-simulate.fx");
            ShaderDraw = Graphics.CreateShader("particles-draw.fx");
            ShaderDraw.ConstantBuffers["CameraConstants"].SetConstantBuffer(cameraConstants.Buffer);
            //Model = new Model(Graphics.Device, Mesh.CreateSphere(3, 3).SplitVertices(true));
            Model = new Model(Graphics.Device, Mesh.CreateGrid(3, 3));

        }

        public void Simulate()
        {
            ShaderSimulate.UnorderedAccesses["Particles"].Set(Buffer.UAView);
            Graphics.Compute(ShaderSimulate, (Buffer.ElementCount, 1, 1));
        }

        public void Draw(ShaderResourceView? texture = null)
        {
            Graphics.SetModel(Model);
            ShaderDraw.ShaderResources["Texture"].SetResource(texture);
            ShaderDraw.ShaderResources["Particles"].SetResource(Buffer.SRView);
            Graphics.Draw(ShaderDraw, Buffer.ElementCount);
        }

        public void Dispose()
        {
            Buffer.Dispose();
            ShaderSimulate.Dispose();
            ShaderDraw.Dispose();
        }
    }
}
