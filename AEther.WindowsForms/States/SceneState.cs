
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

        readonly int InstancingThreshold = 64;
        readonly List<Geometry> Scene = new();
        readonly Shader GeometryShader;
        readonly Shader GeometryShaderInstanced;
        readonly Particles Particles;
        readonly ConstantBuffer<Instance> InstanceConstants;
        readonly ConstantBuffer<CameraConstants> CameraConstants;
        readonly ComputeBuffer Instances;
        readonly Model Cube;
        readonly Model Sphere;
        Texture2D DepthBuffer;
        readonly CameraPerspective Camera;

        readonly Shader MandelboxShader;
        readonly Texture2D MandelboxTexture;

        protected bool IsDisposed;

        readonly Stopwatch Timer = new();

        public SceneState(Graphics graphics)
            : base(graphics)
        {

            MandelboxShader = Graphics.CreateShader("mandelbox.fx");
            MandelboxTexture = Graphics.CreateTexture(1 << 10, 1 << 10, SharpDX.DXGI.Format.R8G8B8A8_UNorm);

            InstanceConstants = new(Graphics.Device);
            CameraConstants = new(Graphics.Device);
            Particles = new Particles(Graphics, 1 << 10, CameraConstants);
            Graphics.OnModeChange += Graphics_OnModeChange;
            GeometryShader = Graphics.CreateShader("geometry.fx");
            GeometryShaderInstanced = Graphics.CreateShader("geometry.fx", new[] { new ShaderMacro("INSTANCING", true) });
            Instances = new ComputeBuffer(Graphics.Device, Marshal.SizeOf<Instance>(), 1 << 10, true);
            GeometryShader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
            GeometryShader.ConstantBuffers[2].SetConstantBuffer(InstanceConstants.Buffer);
            GeometryShaderInstanced.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
            GeometryShaderInstanced.ShaderResources["Instances"].SetResource(Instances.SRView);
            var rng = new Random(0);
            DepthBuffer = CreateDepthBuffer();
            Cube = new Model(Graphics.Device, Mesh.CreateSphere(3, 3).SplitVertices(true));
            Sphere = new Model(Graphics.Device, Mesh.CreateSphere(15, 15));
            Camera = new CameraPerspective
            {
                Position = 30 * Vector3.BackwardLH,
                Target = Vector3.Zero,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            //for (var i = 0; i < Instances.ElementCount; ++i)
            //{
            //    var color = rng.NextVector4(Vector4.Zero, Vector4.One);
            //    var transform = 5f * rng.NextMomentum(1, 1, .1f);
            //    var momentum = 5f * rng.NextMomentum(1, 1, 0);
            //    var model = rng.NextDouble() < .5 ? Cube : Sphere;
            //    var obj = new Geometry(model, color, transform, momentum);
            //    Scene.Add(obj);
            //}
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
                MandelboxTexture.Dispose();
                MandelboxShader.Dispose();
                GeometryShader.Dispose();
                GeometryShaderInstanced.Dispose();
                InstanceConstants.Dispose();
                CameraConstants.Dispose();
                Cube.Dispose();
                Sphere.Dispose();
                Instances.Dispose();
                DepthBuffer.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        void UpdateScene()
        {

            //var t = (DateTime.Now - DateTime.Today).TotalSeconds;

            //Camera.Position = Camera.Position.Length() *
            //(
            //    (float)Math.Cos(t) * Vector3.Right +
            //    (float)Math.Sin(t) * Vector3.ForwardLH
            //);

            float dt;
            if (Timer.IsRunning)
            {
                Timer.Stop();
                dt = (float)Timer.Elapsed.TotalSeconds;
                Timer.Restart();
            }
            else
            {
                dt = 0;
                Timer.Start();
            }

            foreach (var g in Scene)
            {
                g.Acceleration = -1f * g.Transform;
                g.Update(dt);
            }

            Particles.Simulate();

        }

        public override void Render()
        {

            UpdateScene();

            Graphics.SetModel();
            Graphics.SetFullscreenTarget(MandelboxTexture);
            Graphics.Draw(MandelboxShader);

            CameraConstants.Value.View = Camera.View;
            CameraConstants.Value.Projection = Camera.Projection;
            CameraConstants.Update(Graphics.Context);

            Graphics.Context.ClearDepthStencilView(DepthBuffer.DSView, DepthStencilClearFlags.Depth, 1, 0);
            Graphics.Context.ClearRenderTargetView(Graphics.BackBuffer.RTView, Color4.Black);
            Graphics.SetFullscreenTarget(Graphics.BackBuffer, DepthBuffer.DSView);

            foreach (var group in Scene.GroupBy(g => g.Model))
            {

                Graphics.SetModel(group.Key);

                var count = group.Count();
                var useInstancing = InstancingThreshold < count;
                if (useInstancing)
                {
                    using (var map = Instances.Map())
                    {
                        foreach (var obj in group)
                        {
                            map.Write(obj.ToInstance());
                        }
                    }
                    Graphics.Draw(GeometryShaderInstanced, count);
                }
                else
                {
                    foreach (var obj in group)
                    {
                        InstanceConstants.Value = obj.ToInstance();
                        InstanceConstants.Update(Graphics.Context);
                        Graphics.Draw(GeometryShader);
                    }
                }

            }

            Particles.Draw(MandelboxTexture.SRView);

        }

    }
}
