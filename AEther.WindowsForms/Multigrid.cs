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

    public class Multigrid : GraphicsComponent, IPoissonSolver, IDisposable
    {

        internal class CoarseGrid : GraphicsComponent, IDisposable
        {

            Shader ResidualShader => Graphics.Shaders["mg-residual.fx"];
            Shader Copy => Graphics.Shaders["copy.fx"];
            Shader Add => Graphics.Shaders["add.fx"];

            internal readonly Multigrid Solver;
            internal readonly Texture2D Residual;
            internal readonly Texture2D ResidualFine;
            internal readonly Texture2D Solution;
            internal readonly EffectVectorVariable ScaleVariable;

            internal CoarseGrid(Graphics graphics, int width, int height, Vector2 scale)
                : base(graphics)
            {
                Solver = new Multigrid(graphics, width, height, scale);
                Residual = Graphics.CreateTexture(width, height, Format.R16_Float);
                Solution = Graphics.CreateTexture(width, height, Format.R16_Float);
                ResidualFine = Graphics.CreateTexture(2 * width, 2 * height, Format.R16_Float);
                ScaleVariable = ResidualShader.Variables["ScaleInv"].AsVector();
            }

            internal void Solve(Texture2D target, Texture2D solution, MultigridMode mode)
            {

                // Residual

                Graphics.SetFullscreenTarget(ResidualFine);
                ScaleVariable.Set(.5f / Solver.Scale);
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

            public void Dispose()
            {
                Solver.Dispose();
                ScaleVariable.Dispose();
            }

        }

        public const int MinSize = 16;

        public readonly int Width;
        public readonly int Height;

        public readonly Vector2 Scale;

        public int Presmoothing { get; set; } = 1;
        public int Postsmoothing { get; set; } = 1;
        public MultigridMode Mode { get; set; } = MultigridMode.VCycle;

        readonly SOR Relaxation;
        readonly CoarseGrid? Coarse;


        public Multigrid(Graphics graphics, int width, int height, Vector2? scale = null)
            : base(graphics)
        {

            Width = width;
            Height = height;
            Scale = scale ?? Vector2.One;

            Relaxation = new SOR(graphics, Width, Height)
            {
                Omega = 1f,
                Scale = Scale,
            };

            if (MinSize < Math.Min(Width, Height))
            {
                Coarse = new CoarseGrid(graphics, Width / 2, Height / 2, Scale * 2);
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

        public void Dispose()
        {
            Coarse?.Dispose();
        }

    }
}
