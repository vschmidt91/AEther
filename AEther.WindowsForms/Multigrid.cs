using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{
    public class Multigrid : GraphicsComponent, IPoissonSolver
    {

        public readonly int SizeLog;


        readonly Texture2D[] Residual;
        readonly Texture2D[] Target;
        readonly Texture2D[] Solution;
        readonly Texture2D[] SolutionNew;

        Shader CoarseSolver => Graphics.Shader["mg-2x2.fx"];
        Shader RelaxationShader => Graphics.Shader["poisson-gs.fx"];
        Shader ResidualShader => Graphics.Shader["mg-residual.fx"];
        Shader Copy => Graphics.Shader["copy.fx"];
        Shader Add => Graphics.Shader["add.fx"];

        public Multigrid(Graphics graphics, int sizeLog)
            : base(graphics)
        {

            SizeLog = sizeLog;

            var sizes = Enumerable.Range(0, SizeLog).Select(l => 2 << l).ToArray();

            Residual = sizes.Select(s => Graphics.CreateTexture(s, s, Format.R32_Float)).ToArray();
            Target = sizes.Select(s => Graphics.CreateTexture(s, s, Format.R32_Float)).ToArray();
            Solution = sizes.Select(s => Graphics.CreateTexture(s, s, Format.R32_Float)).ToArray();
            SolutionNew = sizes.Select(s => Graphics.CreateTexture(s, s, Format.R32_Float)).ToArray();

        }

        public Texture2D Solve(Texture2D target)
        {

            Graphics.Context.ClearRenderTargetView(Solution[SizeLog - 1].GetRenderTargetView(), new Color4(0f));

            VCycle(target, SizeLog - 1);

            return Solution[SizeLog - 1];

        }

        void Smooth(int sizeLog)
        {

            float scale = 1 << (SizeLog - sizeLog - 1);

            Graphics.SetFullscreenTarget(SolutionNew[sizeLog]);
            RelaxationShader.Variables["Omega"].AsScalar().Set(0.8f);
            RelaxationShader.Variables["Scale"].AsScalar().Set(scale);
            RelaxationShader.Variables["UpdateEven"].AsScalar().Set(true);
            RelaxationShader.Variables["UpdateOdd"].AsScalar().Set(true);
            RelaxationShader.ShaderResources["Solution"].AsShaderResource().SetResource(Solution[sizeLog].GetShaderResourceView());
            RelaxationShader.ShaderResources["Target"].AsShaderResource().SetResource(Target[sizeLog].GetShaderResourceView());
            Graphics.Draw(RelaxationShader);

            (Solution[sizeLog], SolutionNew[sizeLog]) = (SolutionNew[sizeLog], Solution[sizeLog]);

        }

        void VCycle(Texture2D target, int sizeLog)
        {

            float scale = 1 << (SizeLog - 1 - sizeLog);


            // Termination

            if (sizeLog == 0)
            {
                //for (int i = 0; i < 256; ++i)
                //{
                //    Smooth(sizeLog);
                //}
                Graphics.SetFullscreenTarget(Solution[sizeLog]);
                CoarseSolver.Variables["Scale"].AsScalar().Set(scale);
                CoarseSolver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
                Graphics.Draw(CoarseSolver);
                return;
            }

            // Relaxation

            for (int i = 0; i < 1; ++i)
            {
                Smooth(sizeLog);
            }

            // Residual

            Graphics.SetFullscreenTarget(Residual[sizeLog]);
            ResidualShader.Variables["Scale"].AsScalar().Set(scale);
            ResidualShader.ShaderResources["Solution"].AsShaderResource().SetResource(Solution[sizeLog].GetShaderResourceView());
            ResidualShader.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            Graphics.Draw(ResidualShader);

            // Projection

            var residual = Target[sizeLog - 1];
            Graphics.SetFullscreenTarget(residual);

            Copy.ShaderResources["Source"].AsShaderResource().SetResource(Residual[sizeLog].GetShaderResourceView());
            Graphics.Draw(Copy);

            // Recursion

            Graphics.Context.ClearRenderTargetView(Solution[sizeLog - 1].GetRenderTargetView(), new Color4(0f));
            VCycle(residual, sizeLog - 1);

            // Interpolation

            Graphics.SetFullscreenTarget(Solution[sizeLog]);

            Add.ShaderResources["Source"].AsShaderResource().SetResource(Solution[sizeLog - 1].GetShaderResourceView());
            Graphics.Draw(Add);

            // Relaxation

            for (int i = 0; i < 1; ++i)
            {
                Smooth(sizeLog);
            }

        }

    }
}
