
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

        const int ShadowBufferSize = 1 << 10;
        readonly int InstancingThreshold = 0;
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
        readonly List<Geometry> Scene = new();
        readonly List<Light> Lights = new();

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
            Graphics.OnModeChange += Graphics_OnModeChange;

            ShadowShader = Graphics.CreateMetaShader("shadow.fx", "INSTANCING");
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
                Position = 15 * Vector3.BackwardLH + 15 * Vector3.Left,
                AspectRatio = Graphics.BackBuffer.Width / (float)Graphics.BackBuffer.Height,
            };
            Camera.Direction = Vector3.Normalize(Vector3.Zero - Camera.Position);
            for (var i = 0; i < 256; ++i)
            {
                var color = rng.NextVector4(Vector4.Zero, Vector4.One);
                var transform = 10f * rng.NextMomentum(1, 1, 0);
                var momentum = 250f * rng.NextMomentum(1, 1, 0);
                var model = rng.NextDouble() < .5 ? Cube : Sphere;
                var obj = new Geometry(model, color, transform);
                Scene.Add(obj);
            }

            //Scene.Add(new Geometry(floor, Vector4.One, new AffineMomentum(null, null, 3)));

            LightConstants.Value.Projection = TextureCube.Projection;
            LightConstants.Value.Anisotropy = .0f;
            LightConstants.Value.Emission = 0f * Vector3.One;
            LightConstants.Value.Scattering = 0.02f * new Vector3(1, 1, 1);
            LightConstants.Value.Absorption = 0f * Vector3.One;

            CreateTextures();

            Lights = Enumerable.Range(0, 3).Select(i => new Light()).ToList();

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

        private void Graphics_OnModeChange(object? sender, SharpDX.DXGI.ModeDescription mode)
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
                //g.Acceleration = -0.1f * g.Transform;
                g.Update(dt);
            }

            Lights[0].Position =
            10 * (
                (float)Math.Cos(t) * Vector3.BackwardLH +
                (float)Math.Sin(t) * Vector3.Right
            );
            Lights[1].Position =
            10 * (
                (float)Math.Cos(t) * Vector3.ForwardLH +
                (float)Math.Sin(t) * Vector3.Up
            );
            Lights[2].Position =
            10 * (
                (float)Math.Cos(t) * Vector3.ForwardLH +
                (float)Math.Sin(t) * Vector3.Right
            );

            Lights[0].Intensity = 500 * Vector3.UnitX;
            Lights[1].Intensity = 500 * Vector3.UnitY;
            Lights[2].Intensity = 500 * Vector3.UnitZ;

            Lights[0].CastsShadows = true;
            Lights[1].CastsShadows = true;
            Lights[2].CastsShadows = true;

            Lights[0].IsVolumetric = true;
            Lights[1].IsVolumetric = true;
            Lights[2].IsVolumetric = true;

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

        void RenderScene(MetaShader metaShader, params string[] switches)
            => RenderScene(metaShader, (IEnumerable<string>)switches);

        void RenderScene(MetaShader metaShader, IEnumerable<string> switches)
        {
            var models = Scene.Select(g => g.Model).ToHashSet();
            foreach (var model in models)
            {
                Graphics.SetModel(model);
                var group = Scene.Where(g => g.Model == model);
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
            NormalBuffer.Clear();
            ColorBuffer.Clear();
            Graphics.SetRenderTargets(DepthBuffer.DSView, NormalBuffer, ColorBuffer);
            RenderScene(GeometryShader);

            LightBuffer.Clear();

            foreach (var light in Lights)
            {

                LightConstants.Value.Intensity = light.Intensity;
                LightConstants.Value.Position = light.Position;
                LightConstants.Value.Distance = Vector3.Distance(Camera.Position, light.Position);
                LightConstants.Value.ShadowFarPlane = TextureCube.FarPlane;
                LightConstants.Update();

                for (var i = 0; i < 6; ++i)
                {
                    Graphics.Context.ClearDepthStencilView(ShadowBuffer.DSViews[i], DepthStencilClearFlags.Depth, 1, 0);
                }

                if(light.CastsShadows)
                {
                    Graphics.SetViewport(new Viewport(0, 0, ShadowBuffer.Width, ShadowBuffer.Height, 0, 1));
                    //var views = TextureCube.CreateViews(light.Position);

                    for (var i = 0; i < 6; ++i)
                    {
                        LightConstants.Value.View = TextureCube.CreateView(i, light.Position);
                        LightConstants.Update();
                        Graphics.Context.OutputMerger.SetRenderTargets(ShadowBuffer.DSViews[i]);
                        RenderScene(ShadowShader, light.GetSwitches());
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
