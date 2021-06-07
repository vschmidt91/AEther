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

    public class FluidState : GraphicsState
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

        public FluidState(Graphics graphics)
            : base(graphics)
        {
            Input = Graphics.CreateShader("fluid-input.fx");
            Advect = Graphics.CreateShader("fluid-advect.fx");
            Diffuse = Graphics.CreateShader("fluid-diffuse.fx");
            DivergenceShader = Graphics.CreateShader("fluid-divergence.fx");
            Project = Graphics.CreateShader("fluid-project.fx");
            Output = Graphics.CreateShader("fluid-output.fx");

            DiffusionVariable = Diffuse.Variables["Variance"].AsScalar();

            //var width = Graphics.BackBuffer.Width;
            //var height = Graphics.BackBuffer.Height;

            var width = 1 << 10;
            var height = width;

            //PoissonSolver = new SOR(graphics, width, height, Format.R16_Float) { Iterations = 256, Omega = 1.8f };
            PoissonSolver = new Multigrid(graphics, width, height, Format.R16_Float, Vector2.One);

            Velocity = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            VelocityNew = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            Pressure = Graphics.CreateTexture(width, height, Format.R16_Float);
            Divergence = Graphics.CreateTexture(width, height, Format.R16_Float);

        }

        public override void Dispose()
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

        public override void Render()
        {

            RenderInput();
            RenderAdvect();
            if(0 < Diffusion)
            {
                RenderDiffuse();
            }
            RenderProject();
            RenderOutput();

        }

        void RenderInput()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Input.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            //Input.ShaderResources["Spectrum0"].SetResource(Graphics.Spectrum[0].Texture.GetShaderResourceView());
            //Input.ShaderResources["Spectrum1"].SetResource(Graphics.Spectrum[1].Texture.GetShaderResourceView());
            Graphics.Draw(Input);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderAdvect()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Advect.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(Advect);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderDiffuse()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Diffuse.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(Diffuse);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderProject()
        {

            Graphics.SetFullscreenTarget(Divergence);
            DivergenceShader.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(DivergenceShader);

            Pressure.Clear();
            PoissonSolver.Solve(Divergence, Pressure);

            Graphics.SetFullscreenTarget(VelocityNew);
            Project.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            Project.ShaderResources["Pressure"].SetResource(Pressure.GetShaderResourceView());
            Graphics.Draw(Project);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderOutput()
        {

            Graphics.SetFullscreenTarget(Graphics.BackBuffer);
            Output.ShaderResources["Velocity"].SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(Output);

        }

    }
}
