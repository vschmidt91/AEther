using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class SOR : GraphicsComponent, IPoissonSolver, IDisposable
    {

        public int Iterations { get; set; } = 1;

        readonly Texture2D Buffer;
        readonly Shader SolverEven;
        readonly Shader SolverOdd;

        public float Omega
        {
            get => OmegaVariableEven.GetFloat();
            set
            {
                OmegaVariableEven.Set(value);
                OmegaVariableOdd.Set(value);
            }
        }

        readonly EffectScalarVariable OmegaVariableEven;
        readonly EffectScalarVariable OmegaVariableOdd;


        public SOR(Graphics graphics, int width, int height, Format format)
            : base(graphics)
        {
            SolverEven = Graphics.CreateShader("poisson-sor.fx");
            SolverOdd = Graphics.CreateShader("poisson-sor.fx");
            Buffer = Graphics.CreateTexture(width, height, format);

            foreach (var solver in new[] { SolverEven, SolverOdd })
            {
                using var even = solver.Variables["UpdateEven"].AsScalar();
                using var odd = solver.Variables["UpdateOdd"].AsScalar();
                even.Set(solver == SolverEven);
                odd.Set(solver == SolverOdd);
            }

            OmegaVariableEven = SolverEven.Variables["Omega"].AsScalar();
            OmegaVariableOdd = SolverOdd.Variables["Omega"].AsScalar();

            SetScale(Vector2.One);
            Omega = 1f;
        }

        public void SetScale(Vector2 scale)
        {
            var s2 = scale * scale;
            var c = .5f * new Vector3(s2.Y, s2.X, -s2.X * s2.Y) / (s2.X + s2.Y);
            foreach(var solver in new[] { SolverEven, SolverOdd })
            {
                using var v = solver.Variables["Coefficients"].AsVector();
                v.Set(c);
            }
        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Debug.Assert(Buffer.Size == solution.Size);
            Debug.Assert(Buffer.Size == target.Size);

            SolverEven.ShaderResources["Target"].SetResource(target.GetShaderResourceView());
            SolverEven.ShaderResources["Solution"].SetResource(solution.GetShaderResourceView());
            SolverOdd.ShaderResources["Target"].SetResource(target.GetShaderResourceView());
            SolverOdd.ShaderResources["Solution"].SetResource(Buffer.GetShaderResourceView());

            for (var i = 0; i < Iterations; ++i)
            {
                Graphics.SetFullscreenTarget(Buffer);
                Graphics.Draw(SolverEven);
                Graphics.SetFullscreenTarget(solution);
                Graphics.Draw(SolverOdd);
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            OmegaVariableEven.Dispose();
            OmegaVariableOdd.Dispose();
            Buffer.Dispose();
            SolverEven.Dispose();
            SolverOdd.Dispose();
        }

    }
}
