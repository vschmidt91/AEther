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
    public class SORZero : GraphicsComponent, IPoissonSolver
    {

        public float Omega { get; set; } = 1f;
        public float Scale { get; set; } = 1f;

        readonly Shader Solver;

        public SORZero(Graphics graphics)
            : base(graphics)
        {

            Solver = Graphics.Shader["poisson-sor-zero.fx"];

        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Graphics.SetFullscreenTarget(solution);
            Solver.Variables["Scale"].AsScalar().Set(Scale);
            Solver.Variables["Omega"].AsScalar().Set(Omega);
            Solver.ShaderResources["Target"].AsShaderResource().SetResource(target.GetShaderResourceView());
            Graphics.Draw(Solver);

        }

    }
}
