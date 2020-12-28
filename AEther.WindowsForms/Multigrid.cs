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

        internal class CoarseGrid : GraphicsComponent
        {

            internal Multigrid Solver;
            internal Texture2D Residual;
            internal Texture2D Solution;

            internal CoarseGrid(Graphics graphics, int sizeLog, int scaleLog)
                : base(graphics)
            {
                Solver = new Multigrid(graphics, sizeLog, scaleLog);
                Residual = Graphics.CreateTexture(Solver.Size, Solver.Size, Format.R16_Float);
                Solution = Graphics.CreateTexture(Solver.Size, Solver.Size, Format.R16_Float);
            }

            internal void Solve()
            {
                Solver.Solve(Residual, Solution);
            }

        }

        public const int MinSize = 32;

        public readonly int SizeLog;
        public int Size => 1 << SizeLog;

        public readonly int ScaleLog;
        public int Scale => 1 << ScaleLog;


        readonly Texture2D Residual;
        readonly IPoissonSolver Relaxation;
        readonly CoarseGrid? Coarse;

        Shader ResidualShader => Graphics.Shader["mg-residual.fx"];
        Shader Copy => Graphics.Shader["copy.fx"];
        Shader Add => Graphics.Shader["add.fx"];

        public Multigrid(Graphics graphics, int sizeLog, int scaleLog = 0)
            : base(graphics)
        {

            SizeLog = sizeLog;
            ScaleLog = scaleLog;

            //Relaxation = new SOR(graphics, Size, Size)
            //{
            //    Iterations = 2,
            //    Jacobi = true,
            //    Omega = .8f,
            //    Scale = Scale,
            //};
            Relaxation = new SORZero(graphics)
            {
                Omega = .8f,
                Scale = Scale,
            };

            if (MinSize < Size)
            {
                Coarse = new CoarseGrid(graphics, sizeLog - 1, scaleLog + 1);
            }
            else
            {
                Coarse = null;
            }

            Residual = Graphics.CreateTexture(Size, Size, Format.R16_Float);

        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            // Relaxation

            Graphics.Context.ClearRenderTargetView(solution.GetRenderTargetView(), new Color4(0f));
            Relaxation.Solve(target, solution);

            // Termination

            if (Coarse == null)
            {
                return;
            }

            // Residual

            Graphics.SetFullscreenTarget(Residual);
            ResidualShader.Variables["Scale"].AsScalar().Set(Scale);
            ResidualShader.ShaderResources["Solution"].AsShaderResource().SetResource(solution.GetShaderResourceView());
            ResidualShader.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            Graphics.Draw(ResidualShader);

            // Projection

            Graphics.SetFullscreenTarget(Coarse.Residual);
            Copy.ShaderResources["Source"].AsShaderResource().SetResource(Residual.GetShaderResourceView());
            Graphics.Draw(Copy);

            // Recursion

            Coarse.Solve();

            // Interpolation

            Graphics.SetFullscreenTarget(solution);
            Add.ShaderResources["Source"].AsShaderResource().SetResource(Coarse.Solution.GetShaderResourceView());
            Graphics.Draw(Add);

        }

    }
}
