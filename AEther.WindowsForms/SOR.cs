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

        Texture2D Solution1;
        Texture2D Solution2;

        readonly Shader Solver;

        public SOR(Graphics graphics, int width, int height)
            : base(graphics)
        {

            Solution1 = Graphics.CreateTexture(width, height, Format.R16_Float);
            Solution2 = Graphics.CreateTexture(width, height, Format.R16_Float);

            Solver = Graphics.Shader["poisson-sor.fx"];

        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Solver.Variables["Scale"].AsScalar().Set(Scale);
            Solver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            Solver.Variables["Omega"].AsScalar().Set(Omega);
            if (Jacobi)
            {
                Solver.Variables["UpdateEven"].AsScalar().Set(true);
                Solver.Variables["UpdateOdd"].AsScalar().Set(true);
            }

            for (var i = 0; i < Iterations; ++i)
            {

                Texture2D source;
                if (i == 0)
                {
                    source = solution;
                }
                else if ((i & 1) == 0)
                {
                    source = Solution1;
                }
                else
                {
                    source = Solution2;
                }

                Texture2D destination;
                if (i == Iterations - 1)
                {
                    destination = solution;
                }
                else if ((i & 1) == 0)
                {
                    destination = Solution2;
                }
                else
                {
                    destination = Solution1;
                }

                if (!Jacobi)
                {
                    Solver.Variables["UpdateEven"].AsScalar().Set((i & 1) == 0);
                    Solver.Variables["UpdateOdd"].AsScalar().Set((i & 1) == 1);
                }

                Graphics.SetFullscreenTarget(destination);
                Solver.ShaderResources["Solution"].AsShaderResource().SetResource(source.GetShaderResourceView());
                Graphics.Draw(Solver);

            }

        }

    }
}
