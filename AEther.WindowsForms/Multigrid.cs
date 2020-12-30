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

            internal CoarseGrid(Graphics graphics, int width, int height, int scaleLog)
                : base(graphics)
            {
                Solver = new Multigrid(graphics, width, height, scaleLog);
                Residual = Graphics.CreateTexture(width, height, Format.R16_Float);
                Solution = Graphics.CreateTexture(width, height, Format.R16_Float);
                ResidualFine = Graphics.CreateTexture(2 * width, 2 * height, Format.R16_Float);
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

        public const int MinSize = 16;

        public readonly int Width;
        public readonly int Height;

        public readonly int ScaleLog;
        public int Scale => 1 << ScaleLog;

        public int Presmoothing { get; set; } = 1;
        public int Postsmoothing { get; set; } = 1;
        public MultigridMode Mode { get; set; } = MultigridMode.VCycle;

        readonly SOR Relaxation;
        readonly CoarseGrid? Coarse;


        public Multigrid(Graphics graphics, int width, int height, int scaleLog = 0)
            : base(graphics)
        {

            Width = width;
            Height = height;
            ScaleLog = scaleLog;

            Relaxation = new SOR(graphics, width, height)
            {
                Omega = 1f,
                Scale = Scale,
            };

            if (MinSize < Math.Min(width, height))
            {
                Coarse = new CoarseGrid(graphics, width / 2, height / 2, scaleLog + 1);
            }
            else
            {
                Coarse = null;
            }


        }

        public void Solve(Texture2D target, Texture2D destination)
            => Solve(target, destination, Mode);

        public void Solve(Texture2D target, Texture2D destination, MultigridMode mode)
        {

            Graphics.Context.ClearRenderTargetView(destination.GetRenderTargetView(), new Color4(0f));

            // Relaxation
            Relaxation.Iterations = Presmoothing;
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

            // Relaxation
            Relaxation.Iterations = Postsmoothing;
            Relaxation.Solve(target, destination);

        }

    }
}
