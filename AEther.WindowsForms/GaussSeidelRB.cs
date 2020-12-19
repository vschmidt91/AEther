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
    public class GaussSeidelRB : GraphicsComponent, IPoissonSolver
    {

        public int Iterations { get; set; }

        Texture2D Solution;
        Texture2D SolutionNew;

        readonly Shader Solver;

        public GaussSeidelRB(Graphics graphics, int width, int height, int iterations = 256)
            : base(graphics)
        {

            Solution = Graphics.CreateTexture(width, height, Format.R32_Float);
            SolutionNew = Graphics.CreateTexture(width, height, Format.R32_Float);

            Solver = Graphics.Shader["poisson-gs.fx"];

            Iterations = iterations;

        }

        public Texture2D Solve(Texture2D target)
        {

            Graphics.Context.ClearRenderTargetView(Solution.GetRenderTargetView(), new Color4(0f));

            Solver.Variables["Omega"].AsScalar().Set(1.8f);

            for (var i = 0; i < 2 * Iterations; ++i)
            {

                Graphics.SetFullscreenTarget(SolutionNew);
                Solver.ShaderResources["Solution"].AsShaderResource().SetResource(Solution.GetShaderResourceView());
                Solver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
                Solver.Variables["UpdateEven"].AsScalar().Set((i & 1) == 0);
                Solver.Variables["UpdateOdd"].AsScalar().Set((i & 1) == 1);
                Graphics.Draw(Solver);

                (Solution, SolutionNew) = (SolutionNew, Solution);

            }

            return Solution;

        }

    }
}
