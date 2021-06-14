
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assimp.Unmanaged;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace AEther.WindowsForms
{
    public class SceneState : GraphicsState
    {

        readonly int InstancingThreshold = 0;
        readonly Shader PresentShader;
        readonly MetaShader ShadowShader;
        readonly MetaShader GeometryShader;
        readonly Particles Particles;
        readonly ConstantBuffer<Instance> InstanceConstants;
        readonly ConstantBuffer<CameraConstants> CameraConstants;
        readonly ConstantBuffer<LightConstants> LightConstants;
        readonly ComputeBuffer Instances;
        readonly Model Cube;
        readonly Model Sphere;
        Texture2D DepthBuffer;
        readonly CameraPerspective Camera;

        readonly Shader MandelboxShader;
        readonly Texture2D MandelboxTexture;

        protected bool IsDisposed;

        readonly Stopwatch Timer = new();

        Texture2D LightBuffer;
        Texture2D ShadowBuffer;
        Texture2D ShadowDepthBuffer;
        Texture2D[] GeometryBuffer;
        readonly MetaShader LightShader;

        readonly List<Geometry> Scene = new();
        readonly List<PointLight> PointLights = new();
        readonly List<DirectionalLight> DirectionalLights = new();

        public SceneState(Graphics graphics)
            : base(graphics)
        {

            ShadowBuffer = Graphics.CreateTexture(new Texture2DDescription
            {
                Width = 1 << 9,
                Height = 1 << 10,
                Format = SharpDX.DXGI.Format.R16_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

            ShadowDepthBuffer = Graphics.CreateTexture(new Texture2DDescription
            {
                Width = ShadowBuffer.Width,
                Height = ShadowBuffer.Height,
                Format = SharpDX.DXGI.Format.D32_Float,
                BindFlags = BindFlags.DepthStencil,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

            MandelboxShader = Graphics.CreateShader("mandelbox.fx");
            MandelboxTexture = Graphics.CreateTexture(1 << 10, 1 << 10, SharpDX.DXGI.Format.R8G8B8A8_UNorm);

            InstanceConstants = new(Graphics.Device);
            CameraConstants = new(Graphics.Device);
            LightConstants = new(Graphics.Device);
            Particles = new Particles(Graphics, 1 << 10, CameraConstants);
            Graphics.OnModeChange += Graphics_OnModeChange;

            LightShader = Graphics.CreateMetaShader("light.fx", "DIRECTIONAL_LIGHT", "VOLUMETRIC");

            Instances = new ComputeBuffer(Graphics.Device, Marshal.SizeOf<Instance>(), 1 << 10, true);
            ShadowShader = Graphics.CreateMetaShader("shadow.fx", "DIRECTIONAL_LIGHT", "INSTANCING");
            foreach (var shader in ShadowShader.Shaders)
            {
                shader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
                shader.ConstantBuffers[2].SetConstantBuffer(LightConstants.Buffer);
                shader.ConstantBuffers[3].SetConstantBuffer(InstanceConstants.Buffer);
                shader.ShaderResources["Instances"].SetResource(Instances.SRView);
            }

            GeometryShader = Graphics.CreateMetaShader("geometry.fx", "INSTANCING");
            foreach (var shader in GeometryShader.Shaders)
            {
                shader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
                shader.ConstantBuffers[2].SetConstantBuffer(InstanceConstants.Buffer);
                shader.ShaderResources["Instances"].SetResource(Instances.SRView);
                shader.ShaderResources["ColorMap"].SetResource(MandelboxTexture.SRView);
            }
            PresentShader = Graphics.CreateShader("present.fx");
            PresentShader.ShaderResources["Shadow"].SetResource(ShadowBuffer.SRView);


            foreach (var shader in LightShader.Shaders)
            {
                shader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
                shader.ConstantBuffers[2].SetConstantBuffer(LightConstants.Buffer);
                shader.ShaderResources["Shadow"].SetResource(ShadowBuffer.SRView);
            }

            var rng = new Random(0);
            Cube = new Model(Graphics.Device, Mesh.CreateSphere(3, 3).SplitVertices(true));
            Sphere = new Model(Graphics.Device, Mesh.CreateSphere(15, 15));
            var floor = new Model(Graphics.Device, Mesh.CreateGrid(32, 32));
            Camera = new CameraPerspective
            {
                Position = 2 * Vector3.BackwardLH + 8 * Vector3.Left,
                Direction = Vector3.Right,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            for (var i = 0; i < 50; ++i)
            {
                var color = rng.NextVector4(Vector4.Zero, Vector4.One);
                var transform = 5f * rng.NextMomentum(1, 1, .2f);
                var momentum = 5f * rng.NextMomentum(1, 1, 0);
                var model = rng.NextDouble() < .5 ? Cube : Sphere;
                var obj = new Geometry(model, color, transform);
                Scene.Add(obj);
            }

            Scene.Add(new Geometry(floor, Vector4.One, new AffineMomentum(null, null, 3)));

            LightConstants.Value.Anisotropy = .2f;
            LightConstants.Value.Emission = 0f * Vector3.One;
            LightConstants.Value.Scattering = 0.04f * new Vector3(1, 1, 1);
            LightConstants.Value.Absorption = 0f * Vector3.One;

            CreateTextures();

        }

        void CreateTextures()
        {

            DepthBuffer = Graphics.CreateTexture(new Texture2DDescription
            {
                Width = Graphics.BackBuffer.Width,
                Height = Graphics.BackBuffer.Height,
                Format = SharpDX.DXGI.Format.D32_Float,
                BindFlags = BindFlags.DepthStencil,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

            GeometryBuffer = new[]
            {
                Graphics.CreateTexture(new Texture2DDescription
                {
                    Width = Graphics.BackBuffer.Width,
                    Height = Graphics.BackBuffer.Height,
                    Format = SharpDX.DXGI.Format.R16_UNorm,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    ArraySize = 1,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default,
                }),
                Graphics.CreateTexture(new Texture2DDescription
                {
                    Width = Graphics.BackBuffer.Width,
                    Height = Graphics.BackBuffer.Height,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_SNorm,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    ArraySize = 1,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default,
                }),
                Graphics.CreateTexture(new Texture2DDescription
                {
                    Width = Graphics.BackBuffer.Width,
                    Height = Graphics.BackBuffer.Height,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    ArraySize = 1,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default,
                }),
            };

            LightBuffer = Graphics.CreateTexture(new Texture2DDescription
            {
                Width = Graphics.BackBuffer.Width,
                Height = Graphics.BackBuffer.Height,
                Format = SharpDX.DXGI.Format.R11G11B10_Float,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

            foreach (var shader in LightShader.Shaders)
            {
                shader.ShaderResources["Depth"].SetResource(GeometryBuffer[0].SRView);
                shader.ShaderResources["Normal"].SetResource(GeometryBuffer[1].SRView);
                shader.ShaderResources["Color"].SetResource(GeometryBuffer[2].SRView);
                shader.ShaderResources["Depth"].SetResource(GeometryBuffer[0].SRView);
                shader.ShaderResources["Normal"].SetResource(GeometryBuffer[1].SRView);
                shader.ShaderResources["Color"].SetResource(GeometryBuffer[2].SRView);
            }
            PresentShader.ShaderResources["Light"].SetResource(LightBuffer.SRView);

        }

        private void Graphics_OnModeChange(object? sender, SharpDX.DXGI.ModeDescription mode)
        {
            DepthBuffer.Dispose();
            foreach(var b in GeometryBuffer)
            {
                b.Dispose();
            }
            LightBuffer.Dispose();
            CreateTextures();
            Camera.AspectRatio = mode.Width / (float)mode.Height;
        }

        public override void Dispose()
        {
            if(!IsDisposed)
            {
                MandelboxTexture.Dispose();
                MandelboxShader.Dispose();
                GeometryShader.Dispose();
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

            var t = .3 * (DateTime.Now - DateTime.Today).TotalSeconds;

            //Camera.Position = 20 *
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
                //g.Acceleration = -0.1f * (float)(1 + Math.Cos(t)) * g.Transform;
                g.Update(dt);
            }

            DirectionalLights.Clear();
            //DirectionalLights.Add(new()
            //{
            //    Direction =
            //    (
            //        (float)Math.Cos(.2 * t) * Vector3.Right +
            //        (float)Math.Sin(.2 * t) * Vector3.Up
            //    ),
            //    Intensity = 10 * Vector3.UnitX
            //});
            //DirectionalLights.Add(new()
            //{
            //    Direction =
            //    (
            //        (float)Math.Cos(.2 * t) * Vector3.ForwardLH +
            //        (float)Math.Sin(.2 * t) * Vector3.Up
            //    ),
            //    Intensity = 10 * Vector3.UnitY
            //});
            //DirectionalLights.Add(new()
            //{
            //    Direction =
            //    (
            //        (float)Math.Cos(.2 * t) * Vector3.ForwardLH +
            //        (float)Math.Sin(.2 * t) * Vector3.Right
            //    ),
            //    Intensity = 10 * Vector3.UnitZ
            //});

            PointLights.Clear();
            PointLights.Add(new()
            {
                Position =
                10 * (
                    (float)Math.Abs(Math.Cos(t)) * Vector3.BackwardLH +
                    (float)Math.Sin(t) * Vector3.Right
                ),
                Intensity = 1000 * Vector3.One
            });
            //PointLights.Add(new()
            //{
            //    Position =
            //    10 * (
            //        (float)Math.Cos(t) * Vector3.ForwardLH +
            //        (float)Math.Sin(t) * Vector3.Up
            //    ),
            //    Intensity = 10000 * Vector3.UnitY
            //});
            //PointLights.Add(new()
            //{
            //    Position =
            //    10 * (
            //        (float)Math.Cos(t) * Vector3.ForwardLH +
            //        (float)Math.Sin(t) * Vector3.Right
            //    ),
            //    Intensity = 10000 * Vector3.UnitZ
            //});

            //Particles.Simulate();

        }

        public override void ProcessKeyPress(KeyPressEventArgs evt)
        {
            var translationSpeed = 1f;
            var rotationSpeed = .1f;
            switch(evt.KeyChar)
            {
                case 'q':
                    Camera.Position -= translationSpeed * Camera.Direction;
                    break;
                case 'e':
                    Camera.Position += translationSpeed * Camera.Direction;
                    break;
                case 'w':
                    Camera.Direction = Vector3.Transform(Camera.Direction, Quaternion.RotationYawPitchRoll(0, -rotationSpeed, 0));
                    break;
                case 's':
                    Camera.Direction = Vector3.Transform(Camera.Direction, Quaternion.RotationYawPitchRoll(0, +rotationSpeed, 0));
                    break;
                case 'a':
                    Camera.Direction = Vector3.Transform(Camera.Direction, Quaternion.RotationYawPitchRoll(-rotationSpeed, 0, 0));
                    break;
                case 'd':
                    Camera.Direction = Vector3.Transform(Camera.Direction, Quaternion.RotationYawPitchRoll(+rotationSpeed, 0, 0));
                    break;
            }
            base.ProcessKeyPress(evt);
        }

        void RenderScene(MetaShader metaShader, params string[] defines)
        {
            foreach (var group in Scene.GroupBy(g => g.Model))
            {
                Graphics.SetModel(group.Key);
                var count = group.Count();
                var useInstancing = InstancingThreshold < count;
                if(useInstancing)
                {
                    defines = defines.Concat(new[] { "INSTANCING" }).ToArray();
                }
                var shader = metaShader[defines];
                if (useInstancing)
                {
                    using (var map = Instances.Map())
                    {
                        foreach (var obj in group)
                        {
                            map.Write(obj.ToInstance());
                        }
                    }
                    Graphics.Draw(shader, count);
                }
                else
                {
                    foreach (var obj in group)
                    {
                        InstanceConstants.Value = obj.ToInstance();
                        InstanceConstants.Update();
                        Graphics.Draw(shader);
                    }
                }

            }
        }

        public override void Render()
        {

            UpdateScene();

            Graphics.SetModel();
            Graphics.SetFullscreenTarget(MandelboxTexture);
            Graphics.Draw(MandelboxShader);

            var x = Vector3.ForwardLH;
            var bounds = Camera.GetBounds();
            CameraConstants.Value.View = Camera.View;
            CameraConstants.Value.Projection = Camera.Projection;
            CameraConstants.Value.ViewPosition = Camera.Position;
            CameraConstants.Value.FarPlane = Camera.FarPlane;
            CameraConstants.Value.TopLeft = bounds[0, 1, 1];
            CameraConstants.Value.HStep = bounds[1, 1, 1] - bounds[0, 1, 1];
            CameraConstants.Value.VStep = bounds[0, 0, 1] - bounds[0, 1, 1];
            CameraConstants.Update();

            DepthBuffer.ClearDepth();
            GeometryBuffer[0].Clear(new Color4(Camera.FarPlane));
            GeometryBuffer[1].Clear();
            GeometryBuffer[2].Clear();
            Graphics.SetRenderTargets(DepthBuffer, GeometryBuffer);
            RenderScene(GeometryShader);

            LightBuffer.Clear();

            foreach (var light in DirectionalLights)
            {

                LightConstants.Value.Transform = light.GetTransform();
                LightConstants.Value.Intensity = light.Intensity;
                LightConstants.Value.PositionOrDirection = light.Direction;
                LightConstants.Update();

                ShadowDepthBuffer.ClearDepth();
                ShadowBuffer.Clear(Color4.White);
                Graphics.SetRenderTargets(ShadowDepthBuffer, ShadowBuffer);
                RenderScene(ShadowShader, "DIRECTIONAL_LIGHT");

                var shader = LightShader["VOLUMETRIC", "DIRECTIONAL_LIGHT"];
                Graphics.SetModel(null);
                Graphics.SetRenderTargets(null, LightBuffer);
                Graphics.Draw(shader);

            }


            foreach (var light in PointLights)
            {

                LightConstants.Value.Transform = light.GetTransform(Camera.Position);
                LightConstants.Value.Intensity = light.Intensity;
                LightConstants.Value.PositionOrDirection = light.Position;
                LightConstants.Value.Distance = Vector3.Distance(Camera.Position, light.Position);
                LightConstants.Update();

                ShadowDepthBuffer.ClearDepth();
                ShadowBuffer.Clear(Color4.White);
                Graphics.SetRenderTargets(ShadowDepthBuffer, ShadowBuffer);
                RenderScene(ShadowShader);

                var shader = LightShader["VOLUMETRIC"];
                Graphics.SetModel(null);
                Graphics.SetRenderTargets(null, LightBuffer);
                Graphics.Draw(shader);

            }


            Graphics.BackBuffer.Clear();
            Graphics.SetRenderTargets(null, Graphics.BackBuffer);
            Graphics.Draw(PresentShader);

            //Particles.Draw(MandelboxTexture.SRView);

        }

    }
}
