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

        public int Iterations { get; set; } = 1;
        public float Omega { get; set; } = 1f;
        public Vector2 Scale { get; set; } = Vector2.One;

        Texture2D SolutionBuffer;

        Shader Solver => Graphics.Shaders["poisson-sor.fx"];

        public SOR(Graphics graphics, int width, int height)
            : base(graphics)
        {

            SolutionBuffer = Graphics.CreateTexture(width, height, Format.R16_Float);

        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Texture2D from = solution;
            Texture2D to = SolutionBuffer;

            Solver.Variables["Scale"].AsScalar().Set(Scale.X * Scale.Y);
            Solver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            Solver.Variables["Omega"].AsScalar().Set(Omega);

            for (var i = 0; i < 2 * Iterations; ++i)
            {

                Solver.Variables["UpdateEven"].AsScalar().Set((i % 2) == 0);
                Solver.Variables["UpdateOdd"].AsScalar().Set((i % 2) == 1);
                Solver.ShaderResources["Solution"].AsShaderResource().SetResource(from.GetShaderResourceView());
                Graphics.SetFullscreenTarget(to);
                Graphics.Draw(Solver);

                (from, to) = (to, from);

            }

        }

    }
}
