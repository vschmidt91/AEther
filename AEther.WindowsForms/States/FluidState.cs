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

        public const int SizeLog = 10;
        public const int Size = 1 << SizeLog;

        Texture2D Velocity;
        Texture2D VelocityNew;
        readonly Texture2D Divergence;

        Shader Input => Graphics.Shader["fluid-input.fx"];
        Shader Advect => Graphics.Shader["fluid-advect.fx"];
        Shader Diffuse => Graphics.Shader["fluid-diffuse.fx"];
        Shader DivergenceShader => Graphics.Shader["fluid-divergence.fx"];
        Shader Project => Graphics.Shader["fluid-project.fx"];
        Shader Output => Graphics.Shader["fluid-output.fx"];

        readonly IPoissonSolver PoissonSolver;

        public FluidState(Graphics graphics)
            : base(graphics)
        {

            PoissonSolver = new GaussSeidelRB(graphics, Size, Size, 256);
            //PoissonSolver = new Multigrid(graphics, SizeLog);

            Velocity = Graphics.CreateTexture(Size, Size, Format.R32G32B32A32_Float);
            VelocityNew = Graphics.CreateTexture(Size, Size, Format.R32G32B32A32_Float);
            Divergence = Graphics.CreateTexture(Size, Size, Format.R32_Float);

            Graphics.Context.ClearRenderTargetView(Velocity.GetRenderTargetView(), new Color4(0f));


        }

        public override void Render()
        {
            
            RenderInput();
            RenderAdvect();
            RenderDiffuse();
            var pressure = RenderProject();
            RenderOutput(pressure);

        }

        void RenderInput()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Input.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Input.ShaderResources["Spectrum0"].AsShaderResource().SetResource(Graphics.Spectrum[0].Texture.GetShaderResourceView());
            Input.ShaderResources["Spectrum1"].AsShaderResource().SetResource(Graphics.Spectrum[1].Texture.GetShaderResourceView());
            Graphics.Draw(Input);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderAdvect()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Advect.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(Advect);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderDiffuse()
        {

            Graphics.SetFullscreenTarget(VelocityNew);
            Diffuse.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Diffuse.Variables["Size"].AsScalar().Set(Size);
            Graphics.Draw(Diffuse);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        Texture2D RenderProject()
        {

            Graphics.SetFullscreenTarget(Divergence);
            DivergenceShader.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(DivergenceShader);

            var pressure = PoissonSolver.Solve(Divergence);

            Graphics.SetFullscreenTarget(VelocityNew);
            Project.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Project.ShaderResources["Pressure"].AsShaderResource().SetResource(pressure.GetShaderResourceView());
            Graphics.Draw(Project);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

            return pressure;

        }

        void RenderOutput(Texture2D pressure)
        {

            Graphics.SetFullscreenTarget(Graphics.BackBuffer);
            Output.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Output.ShaderResources["Pressure"].AsShaderResource().SetResource(pressure.GetShaderResourceView());
            Graphics.Draw(Output);

        }

    }
}
