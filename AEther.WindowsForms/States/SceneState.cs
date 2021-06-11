
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class SceneState : GraphicsState
    {

        int InstancingThreshold = 64;

        readonly List<Geometry> Scene = new();
        readonly Shader GeometryShader;
        readonly Shader GeometryShaderInstanced;
        readonly ConstantBuffer<Instance> InstanceConstants;
        readonly ConstantBuffer<CameraConstants> CameraConstants;
        readonly ComputeBuffer InstanceBuffer;
        readonly Instance[] Instances = new Instance[1 << 12];
        readonly Model Cube;
        readonly Model Sphere;
        Texture2D DepthBuffer;
        readonly CameraPerspective Camera;

        protected bool IsDisposed;

        readonly Stopwatch Timer = new();

        public SceneState(Graphics graphics)
            : base(graphics)
        {
            Graphics.OnModeChange += Graphics_OnModeChange;
            GeometryShader = Graphics.CreateShader("geometry.fx");
            GeometryShaderInstanced = Graphics.CreateShader("geometry.fx", new[] { new ShaderMacro("INSTANCING", true) });
            InstanceConstants = new(Graphics.Device);
            CameraConstants = new(Graphics.Device);
            InstanceBuffer = new ComputeBuffer(Graphics.Device, Marshal.SizeOf<Instance>(), Instances.Length, true);
            GeometryShader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
            GeometryShader.ConstantBuffers[2].SetConstantBuffer(InstanceConstants.Buffer);
            GeometryShaderInstanced.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
            GeometryShaderInstanced.ShaderResources["Instances"].SetResource(InstanceBuffer.SRView);
            var rng = new Random(0);
            DepthBuffer = CreateDepthBuffer();
            Cube = new Model(Graphics.Device, Mesh.CreateSphere(3, 3));
            Sphere = new Model(Graphics.Device, Mesh.CreateSphere(15, 15));
            Camera = new CameraPerspective
            {
                Position = 20 * Vector3.BackwardLH,
                Target = Vector3.Zero,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            for(var i = 0; i < Instances.Length; ++i)
            {
                var color = rng.NextVector3(Vector3.Zero, Vector3.One);
                var transform = 5f * rng.NextMomentum(1, 1, .1f);
                var momentum = 5f * rng.NextMomentum(1, 1, 0);
                var model = rng.NextDouble() < .5 ? Cube : Sphere;
                var obj = new Geometry(model, color, transform, momentum);
                Scene.Add(obj);
            }
        }

        Texture2D CreateDepthBuffer()
            => Graphics.CreateTexture(new Texture2DDescription
            {
                Width = Graphics.BackBuffer.Width,
                Height = Graphics.BackBuffer.Height,
                Format = SharpDX.DXGI.Format.D32_Float,
                BindFlags = BindFlags.DepthStencil,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 0,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

        private void Graphics_OnModeChange(object? sender, SharpDX.DXGI.ModeDescription mode)
        {
            DepthBuffer.Dispose();
            DepthBuffer = CreateDepthBuffer();
            Camera.AspectRatio = mode.Width / (float)mode.Height;
        }

        public override void Dispose()
        {
            if(!IsDisposed)
            {
                GeometryShader.Dispose();
                GeometryShaderInstanced.Dispose();
                InstanceConstants.Dispose();
                CameraConstants.Dispose();
                Cube.Dispose();
                Sphere.Dispose();
                InstanceBuffer.Dispose();
                DepthBuffer.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        void UpdateScene()
        {

            float dt;
            if (Timer.IsRunning)
            {
                Timer.Stop();
                dt = (float)Timer.Elapsed.TotalSeconds;
                Timer.Restart();
            }
            else
            {
                dt = 0f;
                Timer.Start();
            }

            foreach (var g in Scene)
            {
                g.Acceleration = -1f * g.Transform;
                g.Update(dt);
            }

        }

        public override void Render()
        {

            UpdateScene();

            CameraConstants.Value.View = Camera.View;
            CameraConstants.Value.Projection = Camera.Projection;
            CameraConstants.Update(Graphics.Context);

            Graphics.Context.ClearDepthStencilView(DepthBuffer.DSView, DepthStencilClearFlags.Depth, 1, 0);
            Graphics.Context.ClearRenderTargetView(Graphics.BackBuffer.RTView, Color4.Black);
            Graphics.SetFullscreenTarget(Graphics.BackBuffer, DepthBuffer.DSView);

            foreach (var group in Scene.GroupBy(g => g.Model))
            {

                var count = group.Count();
                var useInstancing = InstancingThreshold < count;

                Graphics.SetModel(group.Key);
                Array.Clear(Instances, 0, Instances.Length);

                foreach (var (obj, i) in group.WithIndex())
                {
                    var instance = new Instance()
                    {
                        Transform = obj.Transform.ToTransform().ToMatrix(),
                        Color = obj.Color,
                    };
                    if (useInstancing)
                    {
                        Instances[i] = instance;
                    }
                    else
                    {
                        InstanceConstants.Value = instance;
                        InstanceConstants.Update(Graphics.Context);
                        Graphics.Draw(GeometryShader);
                    }
                }

                if (useInstancing)
                {
                    InstanceBuffer.Update<Instance>(Instances[0..count]);
                    Graphics.Draw(GeometryShaderInstanced, count);
                }


            }

        }

    }
}
