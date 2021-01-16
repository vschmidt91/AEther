using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            Shader CopyShader => Graphics.Shaders["copy.fx"];
            Shader AddShader => Graphics.Shaders["add.fx"];
            Shader ResidualShader => Graphics.Shaders["mg-residual.fx"];

            readonly Multigrid Solver;
            readonly Texture2D Solution;
            readonly Texture2D Residual;
            readonly Texture2D ResidualFine;
            readonly EffectVectorVariable CoefficientsVariable;

            internal CoarseGrid(Graphics graphics, int width, int height, Format format, Vector2 scale)
                : base(graphics)
            {
                Solver = new Multigrid(graphics, width, height, format, scale);
                Solution = Graphics.CreateTexture(width, height, format);
                Residual = Graphics.CreateTexture(width, height, format);
                ResidualFine = Graphics.CreateTexture(2 * width, 2 * height, format);
                CoefficientsVariable = ResidualShader.Variables["Coefficients"].AsVector();
            }

            internal void Solve(Texture2D targetFine, Texture2D solutionFine, MultigridMode mode)
            {

                Debug.Assert(ResidualFine.Size == solutionFine.Size);
                Debug.Assert(ResidualFine.Size == targetFine.Size);
                Debug.Assert(ResidualFine.Size == 2 * Residual.Size);
                Debug.Assert(Residual.Size == Solution.Size);
                Debug.Assert(solutionFine.Size == 2 * Solution.Size);

                // Residual
                Graphics.SetFullscreenTarget(ResidualFine);
                CoefficientsVariable.Set(4 / (Solver.Scale * Solver.Scale));
                ResidualShader.ShaderResources["Solution"].SetResource(solutionFine.GetShaderResourceView());
                ResidualShader.ShaderResources["Target"].SetResource(targetFine.GetShaderResourceView());
                Graphics.Draw(ResidualShader);


                // Projection
                Graphics.SetFullscreenTarget(Residual);
                CopyShader.ShaderResources["Source"].SetResource(ResidualFine.GetShaderResourceView());
                Graphics.Draw(CopyShader);


                // Recursion
                Solution.Clear();
                Solver.Solve(Residual, Solution, mode);


                // Interpolation
                Graphics.SetFullscreenTarget(solutionFine);
                AddShader.ShaderResources["Source"].SetResource(Solution.GetShaderResourceView());
                Graphics.Draw(AddShader);

            }

            public void Dispose()
            {
                Solver.Dispose();
                Residual.Dispose();
                Solution.Dispose();
                ResidualFine.Dispose();
                CoefficientsVariable.Dispose();
            }

        }

        public const int CoarsestSize = 32;
        public const int CoarsestIterations = 8;

        public readonly int Width;
        public readonly int Height;

        public readonly Vector2 Scale;

        public MultigridMode Mode { get; set; } = MultigridMode.WCycle;

        readonly SOR Relaxation;
        readonly CoarseGrid? Coarse;

        public Multigrid(Graphics graphics, int width, int height, Format format, Vector2? scale = null)
            : base(graphics)
        {

            Width = width;
            Height = height;
            Scale = scale ?? Vector2.One;

            Relaxation = new(graphics, width, height, format)
            {
                Iterations = 1,
                Omega = 1f,
                Scale = Scale,
            };

            var coarseWidth = Width / 2;
            var coarseHeight = Height / 2;
            var coarseScale = Scale * 2;

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

            Debug.Assert(target.Size == solution.Size);

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
            GC.SuppressFinalize(this);
            Coarse?.Dispose();
            Relaxation.Dispose();
        }

    }
}
