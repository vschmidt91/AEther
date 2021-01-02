﻿using System;
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

        public const int Size = 1 << 10;

        Texture2D Velocity;
        Texture2D VelocityNew;
        Texture2D Pressure;
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

            var width = Graphics.BackBuffer.Width;
            var height = Graphics.BackBuffer.Height;

            width = height = Size;

            //PoissonSolver = new SOR(graphics, Size, Size) { Iterations = 256, Omega = 1.8f };
            PoissonSolver = new Multigrid(graphics, width, height);

            Velocity = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            VelocityNew = Graphics.CreateTexture(width, height, Format.R16G16B16A16_Float);
            Pressure = Graphics.CreateTexture(width, height, Format.R16_Float);
            Divergence = Graphics.CreateTexture(width, height, Format.R16_Float);

            Graphics.Context.ClearRenderTargetView(Velocity.GetRenderTargetView(), new Color4(0f));


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
            Input.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            //Input.ShaderResources["Spectrum0"].AsShaderResource().SetResource(Graphics.Spectrum[0].Texture.GetShaderResourceView());
            //Input.ShaderResources["Spectrum1"].AsShaderResource().SetResource(Graphics.Spectrum[1].Texture.GetShaderResourceView());
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

        void RenderProject()
        {

            Graphics.SetFullscreenTarget(Divergence);
            DivergenceShader.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(DivergenceShader);

            PoissonSolver.Solve(Divergence, Pressure);

            Graphics.SetFullscreenTarget(VelocityNew);
            Project.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Project.ShaderResources["Pressure"].AsShaderResource().SetResource(Pressure.GetShaderResourceView());
            Graphics.Draw(Project);

            (Velocity, VelocityNew) = (VelocityNew, Velocity);

        }

        void RenderOutput()
        {

            Graphics.SetFullscreenTarget(Graphics.BackBuffer);
            Output.ShaderResources["Velocity"].AsShaderResource().SetResource(Velocity.GetShaderResourceView());
            Graphics.Draw(Output);

        }

    }
}
