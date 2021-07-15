using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{

    public class Fluid : GraphicsComponent, IDisposable
    {

        readonly EffectScalarVariable DiffusionVariable;
        public float Diffusion
        {
            get => DiffusionVariable.GetFloat();
            set => DiffusionVariable.Set(value);
        }

        Texture2D Velocity;
        Texture2D VelocityNew;
        readonly Texture2D Pressure;
        readonly Texture2D Divergence;
        readonly Shader Input;
        readonly Shader Advect;
        readonly Shader Diffuse;
        readonly Shader DivergenceShader;
        readonly Shader Project;
        readonly Shader Output;
        readonly IPoissonSolver PoissonSolver;

        public Fluid(Graphics graphics, int width, int height)
            : base(graphics)
        {

            Input = Graphics.CreateShader("fluid-input.fx");
            Advect = Graphics.CreateShader("fluid-advect.fx");
            Diffuse = Graphics.CreateShader("fluid-diffuse.fx");
            DivergenceShader = Graphics.CreateShader("fluid-divergence.fx");
            Project = Graphics.CreateShader("fluid-project.fx", new SharpDX.Direct3D.ShaderMacro("FAST_DERIVATIVES", true));
            Output = Graphics.CreateShader("fluid-output.fx");
            DiffusionVariable = Diffuse.Variables["Variance"].AsScalar();

            //PoissonSolver = new SOR(graphics, width, height, Format.R16_Float) { Iterations = 256, Omega = 1.8f };
            PoissonSolver = new Multigrid(graphics, width, height, Format.R16_Float, Vector2.One);

            Velocity = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            VelocityNew = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            Pressure = Graphics.CreateTexture(width, height, Format.R16_Float);
            Divergence = Graphics.CreateTexture(width, height, Format.R16_Float);

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DiffusionVariable.Dispose();
            Velocity.Dispose();
            VelocityNew.Dispose();
            Pressure.Dispose();
            Divergence.Dispose();
            Input.Dispose();
            Advect.Dispose();
            Diffuse.Dispose();
            DivergenceShader.Dispose();
            Project.Dispose();
            Output.Dispose();
        }

        public Texture2D Render()
        {

            RenderInput();
            RenderAdvect();
            if(0 < Diffusion)
            {
                RenderDiffuse();
            }
            RenderProject();
            RenderOutput();

            return Velocity;

        }

        void RenderInput()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Input.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Graphics.Draw(Input);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderAdvect()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Advect.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Graphics.Draw(Advect);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderDiffuse()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Diffuse.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Graphics.Draw(Diffuse);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderProject()
        {

            Graphics.SetFullscreenTarget(Divergence);
            DivergenceShader.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Graphics.Draw(DivergenceShader);

            Pressure.Clear();
            PoissonSolver.Solve(Divergence, Pressure);

            Graphics.SetFullscreenTarget(VelocityNew);
            Project.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Project.ShaderResources["Pressure"].SetResource(Pressure.SRView);
            Graphics.Draw(Project);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderOutput()
        {

            Graphics.SetFullscreenTarget(Graphics.BackBuffer);
            Output.ShaderResources["Velocity"].SetResource(Velocity.SRView);
            Graphics.Draw(Output);

        }

    }
}
