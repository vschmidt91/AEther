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
    public class Particles : GraphicsComponent
    {

        readonly ComputeBuffer Buffer;
        readonly Shader ParticleSimulationShader;
        readonly Shader ParticleDrawShader;
        readonly Model Model;

        public Particles(Graphics graphics, int count, ConstantBuffer<CameraConstants> cameraConstants)
            : base(graphics)
        {

            Buffer = new ComputeBuffer(Graphics.Device, 80, count, false);
            ParticleSimulationShader = Graphics.CreateShader("particles-simulate.fx");
            ParticleDrawShader = Graphics.CreateShader("particles-draw.fx");
            ParticleDrawShader.ConstantBuffers[1].SetConstantBuffer(cameraConstants.Buffer);
            Model = new Model(Graphics.Device, Mesh.CreateSphere(3, 3).SplitVertices(true));

        }

        public void Simulate()
        {
            ParticleSimulationShader.UnorderedAccesses["Particles"].Set(Buffer.UAView);
            Graphics.Compute(ParticleSimulationShader, (Buffer.ElementCount, 1, 1));
        }

        public void Draw(ShaderResourceView? texture = null)
        {
            Graphics.SetModel(Model);
            ParticleDrawShader.ShaderResources["Texture"].SetResource(texture);
            ParticleDrawShader.ShaderResources["Particles"].SetResource(Buffer.SRView);
            Graphics.Draw(ParticleDrawShader, Buffer.ElementCount);
        }

    }
}
