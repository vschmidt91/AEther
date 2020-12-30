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
    public class SOR : GraphicsComponent, IPoissonSolver
    {

        public int Iterations { get; set; } = 2;
        public float Omega { get; set; } = 1f;
        public bool Jacobi { get; set; } = false;
        public float Scale { get; set; } = 1f;

        Texture2D SolutionBuffer;

        Shader Solver => Graphics.Shader["poisson-sor.fx"];
        Shader SolverZero => Graphics.Shader["poisson-sor-zero.fx"];

        public SOR(Graphics graphics, int width, int height)
            : base(graphics)
        {

            SolutionBuffer = Graphics.CreateTexture(width, height, Format.R16_Float);

        }

        public void Solve(Texture2D target, Texture2D destination)
        {

            Texture2D from = SolutionBuffer;
            Texture2D to = destination;

            if((Iterations % 2) == 0)
            {
                (from, to) = (to, from);
            }

            SolverZero.Variables["Scale"].AsScalar().Set(Scale);
            SolverZero.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            SolverZero.Variables["Omega"].AsScalar().Set(Omega);

            Graphics.SetFullscreenTarget(to);
            Graphics.Draw(SolverZero);

            if (1 < Iterations)
            {
                Solver.Variables["Scale"].AsScalar().Set(Scale);
                Solver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
                Solver.Variables["Omega"].AsScalar().Set(Omega);
                if (Jacobi)
                {
                    Solver.Variables["UpdateEven"].AsScalar().Set(true);
                    Solver.Variables["UpdateOdd"].AsScalar().Set(true);
                }
            }

            for (var i = 1; i < Iterations; ++i)
            {

                (from, to) = (to, from);

                Solver.ShaderResources["Solution"].AsShaderResource().SetResource(from.GetShaderResourceView());
                if (!Jacobi)
                {
                    Solver.Variables["UpdateEven"].AsScalar().Set((i & 1) == 0);
                    Solver.Variables["UpdateOdd"].AsScalar().Set((i & 1) == 1);
                }

                Graphics.SetFullscreenTarget(to);
                Graphics.Draw(Solver);

            }

        }

    }
}
