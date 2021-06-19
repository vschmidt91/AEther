
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
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace AEther.WindowsForms
{
    public class SceneState : GraphicsState
    {

        internal class MyLight : Light
        {

            public MyLight()
            {
                Intensity = 1000 * Vector3.One;
                IsVolumetric = true;
                CastsShadows = true;
                Transform.Translation =  10 * Vector3.BackwardLH;
            }

            public override void Update(float dt)
            {
                Vector3 d1 = Vector3.Normalize(Vector3.Cross(Transform.Translation, Vector3.Up));
                Vector3 d2 = Vector3.Normalize(Vector3.Cross(Transform.Translation, d1));
                Momentum.Translation = 3f * d2;
                base.Update(dt);
            }
        }

        internal class MyGeometry : Geometry
        {

            public MyGeometry(Model model)
                : base(model)
            { }

            public override void Update(float dt)
            {
                Acceleration = -.1f * Transform;
                base.Update(dt);
            }

        }

        const int ShadowBufferSize = 1 << 10;
        readonly int InstancingThreshold = 1 << 6;
        bool EquiangularSamplingEnabled = true;

        readonly Shader MandelboxShader;
        readonly Shader PresentShader;
        readonly MetaShader ShadowShader;
        readonly MetaShader GeometryShader;
        readonly MetaShader LightShader;

        readonly Particles Particles;
        readonly ConstantBuffer<Instance> InstanceConstants;
        readonly ConstantBuffer<CameraConstants> CameraConstants;
        readonly ConstantBuffer<LightConstants> LightConstants;
        readonly ComputeBuffer Instances;
        readonly Model Cube;
        readonly Model Sphere;

        DepthBuffer DepthBuffer;
        Texture2D NormalBuffer;
        Texture2D ColorBuffer;
        Texture2D LightBuffer;

        readonly CameraPerspective Camera;
        readonly TextureCube ShadowBuffer;
        readonly Texture2D MandelboxTexture;

        protected bool IsDisposed;
        readonly Stopwatch Timer = new();
        readonly List<SceneNode> Scene = new();

        public SceneState(Graphics graphics)
            : base(graphics)
        {

            ShadowBuffer = new TextureCube(Graphics, ShadowBufferSize, ShadowBufferSize, Format.R32_Typeless);

            MandelboxShader = Graphics.CreateShader("mandelbox.fx");
            MandelboxTexture = Graphics.CreateTexture(1 << 10, 1 << 10, SharpDX.DXGI.Format.R8G8B8A8_UNorm);

            InstanceConstants = new(Graphics.Device);
            CameraConstants = new(Graphics.Device);
            LightConstants = new(Graphics.Device);
            Instances = new ComputeBuffer(Graphics.Device, Marshal.SizeOf<Instance>(), 1 << 10, true);
            Particles = new Particles(Graphics, 1 << 10, CameraConstants);

            ShadowShader = Graphics.CreateMetaShader("shadow.fx", "INSTANCING");
            foreach (var shader in ShadowShader.Shaders)
            {
                shader.ConstantBuffers[1].SetConstantBuffer(CameraConstants.Buffer);
                shader.ConstantBuffers[2].SetConstantBuffer(LightConstants.Buffer);
                shader.ConstantBuffers[3].SetConstantBuffer(InstanceConstants.Buffer);
                shader.ShaderResources["Instances"].SetResource(Instances.SRView);
                shader.ShaderResources["ColorMap"].SetResource(MandelboxTexture.SRView);
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

            LightShader = Graphics.CreateMetaShader("light.fx", "EQUIANGULAR");
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
                Position = 20 * Vector3.BackwardLH + 20 * Vector3.Left,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            Camera.Direction = Vector3.Normalize(Vector3.Zero - Camera.Position);
            for (var i = 0; i < 1 << 8; ++i)
            {
                var model = rng.NextDouble() < .5 ? Cube : Sphere;
                var obj = new MyGeometry(model)
                {
                    Color = new Vector4(rng.NextVector3(Vector3.Zero, Vector3.One), rng.NextDouble() < .5 ? rng.NextFloat(0, 1) : 1),
                    Transform = 10f * rng.NextMomentum(1, 1, .1f),
                    Momentum = 5f * rng.NextMomentum(1, 1, 0),
                    Roughness = rng.NextFloat(0, 1),
                };
                Scene.Add(obj);
            }
            Scene.Add(new MyLight());

            Scene.Add(new Geometry(floor)
            {
                Color = Vector4.One,
                Transform = new()
                {
                    ScaleLog = 3,
                },
            });

            LightConstants.Value.Projection = TextureCube.Projection;
            LightConstants.Value.Anisotropy = .0f;
            LightConstants.Value.Emission = 0f * Vector3.One;
            LightConstants.Value.Scattering = 0.02f * new Vector3(1, 1, 1);
            LightConstants.Value.Absorption = 0f * Vector3.One;

            CreateTextures();

            Graphics.OnModeChange += Graphics_OnModeChange;

        }

        void CreateTextures()
        {

            DepthBuffer = Graphics.CreateDepthBuffer(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R16_Typeless);
            NormalBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R8G8B8A8_SNorm);
            ColorBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R8G8B8A8_UNorm);
            LightBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R11G11B10_Float);

            foreach (var shader in LightShader.Shaders)
            {
                shader.ShaderResources["Depth"].SetResource(DepthBuffer.SRView);
                shader.ShaderResources["Normal"].SetResource(NormalBuffer.SRView);
                shader.ShaderResources["Color"].SetResource(ColorBuffer.SRView);
            }
            PresentShader.ShaderResources["Light"].SetResource(LightBuffer.SRView);

        }

        private void Graphics_OnModeChange(object? sender, ModeDescription mode)
        {
            DepthBuffer.Dispose();
            NormalBuffer.Dispose();
            ColorBuffer.Dispose();
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
                g.Update(dt);
            }

            Particles.Simulate();

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
                case 'x':
                    EquiangularSamplingEnabled ^= true;
                    break;
            }
            base.ProcessKeyPress(evt);
        }

        void RenderScene(IEnumerable<Geometry> geometry, MetaShader metaShader, params string[] switches)
            => RenderScene(geometry, metaShader, (IEnumerable<string>)switches);

        void RenderScene(IEnumerable<Geometry> geometry, MetaShader metaShader, IEnumerable<string> switches)
        {
            var models = geometry.Select(g => g.Model).ToHashSet();
            foreach (var model in models)
            {
                Graphics.SetModel(model);
                var group = geometry.Where(g => g.Model == model);
                var count = group.Count();
                var useInstancing = InstancingThreshold < count;
                if(useInstancing)
                {
                    switches = switches.Concat(Enumerable.Repeat("INSTANCING", 1));
                }
                var shader = metaShader[switches];
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

            CameraConstants.Value.View = Camera.View;
            CameraConstants.Value.Projection = Camera.Projection;
            CameraConstants.Value.ViewPosition = Camera.Position;
            CameraConstants.Value.FarPlane = Camera.FarPlane;
            CameraConstants.Value.FarPosMatrix = Camera.GetFarPosMatrix();
            CameraConstants.Update();

            DepthBuffer.ClearDepth();
            NormalBuffer.Clear(new Color4(0, 0, 1, 0));
            ColorBuffer.Clear();
            Graphics.SetRenderTargets(DepthBuffer.DSView, NormalBuffer, ColorBuffer);
            RenderScene(Scene.OfType<Geometry>(), GeometryShader);

            LightBuffer.Clear();

            foreach (var light in Scene.OfType<Light>())
            {

                LightConstants.Value.Intensity = light.Intensity;
                LightConstants.Value.Position = light.Transform.Translation;
                LightConstants.Value.Distance = Vector3.Distance(Camera.Position, light.Transform.Translation);
                LightConstants.Value.ShadowFarPlane = TextureCube.FarPlane;
                LightConstants.Update();

                for (var i = 0; i < 6; ++i)
                {
                    Graphics.Context.ClearDepthStencilView(ShadowBuffer.DSViews[i], DepthStencilClearFlags.Depth, 1, 0);
                }

                if(light.CastsShadows)
                {
                    Graphics.SetViewport(new Viewport(0, 0, ShadowBuffer.Width, ShadowBuffer.Height, 0, 1));
                    for (var i = 0; i < 6; ++i)
                    {
                        LightConstants.Value.View = TextureCube.CreateView(i, light.Transform.Translation);
                        LightConstants.Update();
                        Graphics.Context.OutputMerger.SetRenderTargets(ShadowBuffer.DSViews[i]);
                        RenderScene(Scene.OfType<Geometry>(), ShadowShader, light.GetSwitches());
                    }
                }

                var defines = light.GetSwitches();
                if (EquiangularSamplingEnabled)
                    defines = defines.Concat(Enumerable.Repeat("EQUIANGULAR", 1));
                var shader = LightShader[defines];
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
