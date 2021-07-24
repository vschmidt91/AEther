
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
                Momentum.Translation = 1f * d2;
                base.Update(dt);
            }
        }

        internal class SpectrumLight : Light
        {

            readonly int Note;

            public SpectrumLight(SpectrumAccumulator<float> spectrum, int note, Vector3 offset, Vector3 direction)
            {
                spectrum.OnUpdate += Spectrum_OnUpdate;
                Note = note;
                IsVolumetric = true;
                CastsShadows = false;
                Transform.Translation = offset + .3f * note * direction;
                Intensity = 100 * Vector3.One;
            }

            private void Spectrum_OnUpdate(object? sender, EventArgs e)
            {
                if (sender is SpectrumAccumulator<float> spectrum)
                {
                    var v = spectrum.Buffer[(4 * Note)..(4 * Note + 3)];
                    Intensity = 100f * new Vector3(v);
                    //Intensity.Y = 0;
                }
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
        const int AirlightSize = 1 << 6;

        readonly Shader MandelboxShader;
        readonly Shader PresentShader;
        readonly Shader AirlightShader;
        readonly Shader ShadowShader;
        readonly Shader GeometryShader;
        readonly Shader LightShader;
        readonly Shader LightShadowShader;

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
        Texture2D AirlightTexture;
        readonly TextureCube ShadowBuffer;
        readonly Texture2D MandelboxTexture;

        readonly CameraPerspective Camera;

        protected bool IsDisposed;
        readonly Stopwatch Timer = new();
        readonly List<SceneNode> Scene = new();
        readonly List<Model> Models = new();
        readonly Fluid Fluid;

        public SceneState(Graphics graphics, SpectrumAccumulator<float>[] spectra)
            : base(graphics)
        {

            ShadowBuffer = Graphics.CreateTextureCube(ShadowBufferSize, ShadowBufferSize, Format.R32_Typeless);

            MandelboxShader = Graphics.CreateShader("mandelbox.fx");
            MandelboxTexture = Graphics.CreateTexture(1 << 10, 1 << 10, SharpDX.DXGI.Format.R8G8B8A8_UNorm);

            InstanceConstants = Graphics.CreateConstantBuffer<Instance>();
            CameraConstants = Graphics.CreateConstantBuffer<CameraConstants>();
            LightConstants = Graphics.CreateConstantBuffer<LightConstants>();
            Instances = Graphics.CreateComputeBuffer(Marshal.SizeOf<Instance>(), 1 << 10, true);
            Particles = new Particles(Graphics, 1 << 10, CameraConstants);

            ShadowShader = Graphics.CreateShader("shadow.fx", new ShaderMacro("ENABLE_INSTANCING", true));
            ShadowShader.ConstantBuffers["CameraConstants"].SetConstantBuffer(CameraConstants.Buffer);
            ShadowShader.ConstantBuffers["LightConstants"].SetConstantBuffer(LightConstants.Buffer);
            ShadowShader.ConstantBuffers["InstanceConstants"].SetConstantBuffer(InstanceConstants.Buffer);
            ShadowShader.ShaderResources["Instances"].SetResource(Instances.SRView);
            ShadowShader.ShaderResources["ColorMap"].SetResource(MandelboxTexture.SRView);

            GeometryShader = Graphics.CreateShader("geometry.fx", new ShaderMacro("ENABLE_INSTANCING", true));
            GeometryShader.ConstantBuffers["CameraConstants"].SetConstantBuffer(CameraConstants.Buffer);
            GeometryShader.ConstantBuffers["InstanceConstants"].SetConstantBuffer(InstanceConstants.Buffer);
            GeometryShader.ShaderResources["Instances"].SetResource(Instances.SRView);
            GeometryShader.ShaderResources["ColorMap"].SetResource(MandelboxTexture.SRView);

            PresentShader = Graphics.CreateShader("present.fx");

            AirlightShader = Graphics.CreateShader("airlight.fx");
            AirlightShader.ConstantBuffers["CameraConstants"].SetConstantBuffer(CameraConstants.Buffer);
            AirlightShader.ConstantBuffers["LightConstants"].SetConstantBuffer(LightConstants.Buffer);
            AirlightTexture = Graphics.CreateTexture(AirlightSize, AirlightSize, Format.R11G11B10_Float);

            LightShader = Graphics.CreateShader("light.fx", new ShaderMacro("EQUIANGULAR", true));
            LightShadowShader = Graphics.CreateShader("light.fx", new ShaderMacro("ENABLE_SHADOWS", true), new ShaderMacro("EQUIANGULAR", true));
            foreach (var shader in new[] { LightShader, LightShadowShader })
            {
                shader.ConstantBuffers["CameraConstants"].SetConstantBuffer(CameraConstants.Buffer);
                shader.ConstantBuffers["LightConstants"].SetConstantBuffer(LightConstants.Buffer);
                shader.ShaderResources["Shadow"].SetResource(ShadowBuffer.SRView);
                shader.ShaderResources["Airlight"].SetResource(AirlightTexture.SRView);
            }

            var rng = new Random(0);
            Cube = Graphics.CreateModel(Mesh.CreateSphere(3, 3).SplitVertices(true));
            Sphere = Graphics.CreateModel(Mesh.CreateSphere(15, 15));
            var floor = Graphics.CreateModel(Mesh.CreateGrid(32, 32));
            Camera = new CameraPerspective
            {
                Position = 20 * Vector3.BackwardLH,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            Camera.Direction = Vector3.Normalize(Vector3.Zero - Camera.Position);
            for (var i = 0; i < 1 << 8; ++i)
            {
                var model = rng.NextDouble() < .5 ? Cube : Sphere;
                var obj = new MyGeometry(model)
                {
                    Color = new Vector4(rng.NextVector3(Vector3.Zero, Vector3.One), rng.NextDouble() < .5 ? rng.NextFloat(0, 1) : 1),
                    Transform = rng.NextMomentum(10f, 1, 0),
                    Momentum = rng.NextMomentum(3f, .1f, 0),
                    Roughness = rng.NextFloat(0, 1),
                };
                Scene.Add(obj);
            }
            Scene.Add(new MyLight());

            //for (var c = 0; c < 2; c++)
            //{
            //    var spectrum = spectra[c];
            //    var offset = 5 * (c == 0 ? Vector3.Left : Vector3.Right) + 20 * Vector3.Down;
            //    for (var i = 0; i < spectrum.NoteCount; i++)
            //    {
            //        Scene.Add(new SpectrumLight(spectrum, i, offset, Vector3.Up));
            //    }
            //}

            //Scene.Add(new Geometry(floor)
            //{
            //    Color = Vector4.One,
            //    Transform = new()
            //    {
            //        ScaleLog = 3,
            //    },
            //});

            LightConstants.Value.Projection = TextureCube.Projection;
            LightConstants.Value.Anisotropy = 0f;
            LightConstants.Value.Emission = 0f * Vector3.One;
            LightConstants.Value.Scattering = 0.02f * new Vector3(1, 2, 3);
            LightConstants.Value.Absorption = 0f * Vector3.One;

            CreateTextures();

            Graphics.OnModeChange += Graphics_OnModeChange;

            Models = Scene.OfType<Geometry>().Select(g => g.Model).ToHashSet().ToList();

            int size = 1 << 10;
            Fluid = new Fluid(Graphics, size, size);


        }

        void CreateTextures()
        {

            DepthBuffer = Graphics.CreateDepthBuffer(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R16_Typeless);
            NormalBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R8G8B8A8_SNorm);
            ColorBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R8G8B8A8_UNorm);
            LightBuffer = Graphics.CreateTexture(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height, Format.R11G11B10_Float);

            foreach (var shader in new[] { LightShader, LightShadowShader })
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

                MandelboxShader.Dispose();
                PresentShader.Dispose();
                AirlightShader.Dispose();
                ShadowShader.Dispose();
                GeometryShader.Dispose();
                LightShader.Dispose();
                LightShadowShader.Dispose();

                Particles.Dispose();
                InstanceConstants.Dispose();
                CameraConstants.Dispose();
                LightConstants.Dispose();
                Instances.Dispose();
                Cube.Dispose();
                Sphere.Dispose();

                DepthBuffer.Dispose();
                NormalBuffer.Dispose();
                ColorBuffer.Dispose();
                LightBuffer.Dispose();
                AirlightTexture.Dispose();
                ShadowBuffer.Dispose();
                MandelboxTexture.Dispose();

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
                dt = Math.Min(.1f, (float)Timer.Elapsed.TotalSeconds);
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
            }
            base.ProcessKeyPress(evt);
        }

        void RenderScene(IEnumerable<Geometry> geometry, Shader shader)
        {
            foreach (var model in Models)
            {
                Graphics.SetModel(model);
                var group = geometry.Where(g => g.Model == model);
                var count = group.Count();
                //var useInstancing = true;
                //if (useInstancing)
                //{
                    using (var map = Instances.Map())
                    {
                        foreach (var obj in group)
                        {
                            map.Write(obj.ToInstance());
                        }
                    }
                    Graphics.Draw(shader, count);
                //}
                //else
                //{
                //    foreach (var obj in group)
                //    {
                //        InstanceConstants.Value = obj.ToInstance();
                //        InstanceConstants.Update();
                //        Graphics.Draw(shader);
                //    }
                //}

            }
        }

        public override void Render()
        {

            var fluid = Fluid.Render();
            GeometryShader.ShaderResources["ColorMap"].SetResource(fluid.SRView);

            UpdateScene();

            Graphics.SetModel();
            Graphics.SetFullscreenTarget(AirlightTexture);
            Graphics.Draw(AirlightShader);

            Graphics.SetModel();
            Graphics.SetFullscreenTarget(MandelboxTexture);
            Graphics.Draw(MandelboxShader);

            CameraConstants.Value.View = Camera.View;
            CameraConstants.Value.Projection = Camera.Projection;
            CameraConstants.Value.ViewPosition = Camera.Position;
            CameraConstants.Value.FarPlane = Camera.FarPlane;
            CameraConstants.Value.ViewDirectionMatrix = Camera.GetViewDirectionMatrix();
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

                ShadowBuffer.ClearDepth(1);

                if (light.CastsShadows)
                {
                    Graphics.SetViewport(new Viewport(0, 0, ShadowBuffer.Width, ShadowBuffer.Height, 0, 1));
                    for (var i = 0; i < 6; ++i)
                    {
                        LightConstants.Value.View = TextureCube.CreateView(i, light.Transform.Translation);
                        LightConstants.Update();
                        Graphics.SetRenderTargets(ShadowBuffer.DSViews[i]);
                        //Graphics.Context.OutputMerger.SetRenderTargets(ShadowBuffer.DSViews[i]);
                        RenderScene(Scene.OfType<Geometry>(), ShadowShader);
                    }
                }

                var shader = light.CastsShadows ? LightShadowShader : LightShader;
                Graphics.SetModel(null);
                Graphics.SetRenderTargets(null, LightBuffer);
                Graphics.Draw(shader);

            }


            Graphics.BackBuffer.Clear();
            Graphics.SetRenderTargets(null, Graphics.BackBuffer);
            Graphics.Draw(PresentShader);
            //Graphics.Draw(AirlightShader);

            //Particles.Draw(MandelboxTexture.SRView);

        }

    }
}
