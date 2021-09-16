using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Numerics;

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

            readonly Shader CopyShader;
            readonly Shader AddShader;
            readonly Shader ResidualShader;

            readonly Multigrid Solver;
            readonly Texture2D Solution;
            readonly Texture2D Residual;
            readonly Texture2D ResidualFine;

            bool IsDisposed;

            internal CoarseGrid(Graphics graphics, int width, int height, Format format, Vector2 scale)
                : base(graphics)
            {
                CopyShader = Graphics.LoadShader("copy.fx");
                AddShader = Graphics.LoadShader("add.fx");
                ResidualShader = Graphics.LoadShader("mg-residual.fx");
                Solver = new Multigrid(graphics, width, height, format, scale);
                Solution = Graphics.CreateTexture(width, height, format);
                Residual = Graphics.CreateTexture(width, height, format);
                ResidualFine = Graphics.CreateTexture(2 * width, 2 * height, format);

                using var coeffsVariable = ResidualShader.Variables["Coefficients"].AsVector();
                coeffsVariable.Set(new Vector2(4) / (scale * scale));
            }

            internal void Solve(Texture2D targetFine, Texture2D solutionFine, MultigridMode mode)
            {

                Debug.Assert(ResidualFine.Size == solutionFine.Size);
                Debug.Assert(ResidualFine.Size == targetFine.Size);
                Debug.Assert(ResidualFine.Size == 2 * Residual.Size);
                Debug.Assert(Residual.Size == Solution.Size);
                Debug.Assert(solutionFine.Size == 2 * Solution.Size);

                Graphics.SetModel();

                // Residual
                Graphics.SetRenderTarget(null, ResidualFine);
                ResidualShader.ShaderResources["Solution"].SetResource(solutionFine.SRView);
                ResidualShader.ShaderResources["Target"].SetResource(targetFine.SRView);
                Graphics.Draw(ResidualShader);

                // Projection
                Graphics.SetRenderTarget(null, Residual);
                CopyShader.ShaderResources["Source"].SetResource(ResidualFine.SRView);
                Graphics.Draw(CopyShader);

                // Recursion
                Solution.Clear();
                Solver.Solve(Residual, Solution, mode);

                // Interpolation
                Graphics.SetRenderTarget(null, solutionFine);
                AddShader.ShaderResources["Source"].SetResource(Solution.SRView);
                Graphics.Draw(AddShader);

            }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    CopyShader.Dispose();
                    ResidualShader.Dispose();
                    AddShader.Dispose();
                    Solver.Dispose();
                    Residual.Dispose();
                    Solution.Dispose();
                    ResidualFine.Dispose();
                    GC.SuppressFinalize(this);
                    IsDisposed = true;
                }
            }

        }

        public const int CoarsestSize = 32;
        public const int CoarsestIterations = 8;

        public readonly int Width;
        public readonly int Height;

        public MultigridMode Mode { get; set; } = MultigridMode.VCycle;

        readonly SOR Relaxation;
        readonly CoarseGrid? Coarse;

        protected bool IsDisposed;

        public Multigrid(Graphics graphics, int width, int height, Format format, Vector2 scale)
            : base(graphics)
        {

            Width = width;
            Height = height;

            Relaxation = new(graphics, width, height, format)
            {
                Iterations = 1,
                Scale = scale,
                Omega = 1f,
            };

            var coarseWidth = Width / 2;
            var coarseHeight = Height / 2;
            var coarseScale = scale * 2;

            if (CoarsestSize < Math.Min(coarseWidth, coarseHeight))
            {
                Coarse = new(graphics, coarseWidth, coarseHeight, format, coarseScale);
            }
            else
            {
                Relaxation.Iterations = CoarsestIterations;
            }


        }

        public void Solve(Texture2D target, Texture2D destination)
        {
            Solve(target, destination, Mode);
        }

        public void Solve(Texture2D target, Texture2D solution, MultigridMode mode)
        {

            if (Coarse is null)
            {
                Relaxation.Solve(target, solution);
            }
            else
            {

                Relaxation.Solve(target, solution);

                switch (mode)
                {
                    case MultigridMode.VCycle:
                        Coarse.Solve(target, solution, MultigridMode.VCycle);
                        break;
                    case MultigridMode.FCycle:
                        Coarse.Solve(target, solution, MultigridMode.FCycle);
                        Relaxation.Solve(target, solution);
                        Coarse.Solve(target, solution, MultigridMode.VCycle);
                        break;
                    case MultigridMode.WCycle:
                        Coarse.Solve(target, solution, MultigridMode.WCycle);
                        Relaxation.Solve(target, solution);
                        Coarse.Solve(target, solution, MultigridMode.WCycle);
                        break;
                }

                Relaxation.Solve(target, solution);

            }

        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Coarse?.Dispose();
                Relaxation.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
