using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{

    public class FluidState : GraphicsState
    {

        Texture2D Velocity;
        Texture2D VelocityNew;
        readonly Texture2D Pressure;
        readonly Texture2D Divergence;

        Shader Input => Graphics.Shaders["fluid-input.fx"];
        Shader Advect => Graphics.Shaders["fluid-advect.fx"];
        Shader Diffuse => Graphics.Shaders["fluid-diffuse.fx"];
        Shader DivergenceShader => Graphics.Shaders["fluid-divergence.fx"];
        Shader Project => Graphics.Shaders["fluid-project.fx"];
        Shader Output => Graphics.Shaders["fluid-output.fx"];

        readonly IPoissonSolver PoissonSolver;

        public FluidState(Graphics graphics)
            : base(graphics)
        {

            //var width = Graphics.BackBuffer.Width;
            //var height = Graphics.BackBuffer.Height;

            var width = 1 << 10;
            var height = width;

            //PoissonSolver = new SOR(graphics, width, height, Format.R16_Float) { Iterations = 256, Omega = 1.8f };
            PoissonSolver = new Multigrid(graphics, width, height, Format.R16_Float);

            Velocity = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            VelocityNew = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            Pressure = Graphics.CreateTexture(width, height, Format.R16_Float);
            Divergence = Graphics.CreateTexture(width, height, Format.R16_Float);

        }

        public override void Render()
        {

            RenderInput();
            RenderAdvect();
            //RenderDiffuse();
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
