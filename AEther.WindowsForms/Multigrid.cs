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

    public enum MultigridMode
    {
        VCycle,
        FCycle,
        WCycle,
    }

    public class Multigrid : GraphicsComponent, IPoissonSolver
    {

        internal class CoarseGrid : GraphicsComponent
        {

            Shader ResidualShader => Graphics.Shader["mg-residual.fx"];
            Shader Copy => Graphics.Shader["copy.fx"];
            Shader Add => Graphics.Shader["add.fx"];

            internal readonly Multigrid Solver;
            internal readonly Texture2D Residual;
            internal readonly Texture2D ResidualFine;
            internal readonly Texture2D Solution;

            internal CoarseGrid(Graphics graphics, int sizeLog, int scaleLog)
                : base(graphics)
            {
                Solver = new Multigrid(graphics, sizeLog, scaleLog);
                Residual = Graphics.CreateTexture(Solver.Size, Solver.Size, Format.R16_Float);
                Solution = Graphics.CreateTexture(Solver.Size, Solver.Size, Format.R16_Float);
                ResidualFine = Graphics.CreateTexture(2 * Solver.Size, 2 * Solver.Size, Format.R16_Float);
            }

            internal void Solve(Texture2D target, Texture2D solution, MultigridMode mode)
            {

                // Residual

                Graphics.SetFullscreenTarget(ResidualFine);
                ResidualShader.Variables["Scale"].AsScalar().Set(Solver.Scale / 2);
                ResidualShader.ShaderResources["Solution"].AsShaderResource().SetResource(solution.GetShaderResourceView());
                ResidualShader.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
                Graphics.Draw(ResidualShader);

                // Projection

                Graphics.SetFullscreenTarget(Residual);
                Copy.ShaderResources["Source"].AsShaderResource().SetResource(ResidualFine.GetShaderResourceView());
                Graphics.Draw(Copy);

                // Recursion

                Solver.Solve(Residual, Solution, mode);

                // Interpolation

                Graphics.SetFullscreenTarget(solution);
                Add.ShaderResources["Source"].AsShaderResource().SetResource(Solution.GetShaderResourceView());
                Graphics.Draw(Add);

            }
        }

        public const int MinSize = 8;

        public readonly int SizeLog;
        public int Size => 1 << SizeLog;

        public readonly int ScaleLog;
        public int Scale => 1 << ScaleLog;

        readonly IPoissonSolver Relaxation;
        readonly CoarseGrid? Coarse;


        public Multigrid(Graphics graphics, int sizeLog, int scaleLog = 0)
            : base(graphics)
        {

            SizeLog = sizeLog;
            ScaleLog = scaleLog;

            Relaxation = new SOR(graphics, Size, Size)
            {
                Iterations = 4,
                Jacobi = true,
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


        }

        public void Solve(Texture2D target, Texture2D destination)
            => Solve(target, destination, MultigridMode.FCycle);

        public void Solve(Texture2D target, Texture2D destination, MultigridMode mode)
        {

            // Relaxation

            Graphics.Context.ClearRenderTargetView(destination.GetRenderTargetView(), new Color4(0f));
            Relaxation.Solve(target, destination);

            if (Coarse != null)
            {

                // Recursion
                switch(mode)
                {
                    case MultigridMode.VCycle:
                        Coarse.Solve(target, destination, MultigridMode.VCycle);
                        break;
                    case MultigridMode.FCycle:
                        Coarse.Solve(target, destination, MultigridMode.FCycle);
                        Coarse.Solve(target, destination, MultigridMode.VCycle);
                        break;
                    case MultigridMode.WCycle:
                        Coarse.Solve(target, destination, MultigridMode.WCycle);
                        Coarse.Solve(target, destination, MultigridMode.WCycle);
                        break;
                }

            }

        }

    }
}
