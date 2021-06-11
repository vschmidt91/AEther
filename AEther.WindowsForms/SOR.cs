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

        Vector2 _Scale;
        public Vector2 Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                var s2 = value * value;
                var c = .5f * new Vector3(s2.Y, s2.X, -s2.X * s2.Y) / (s2.X + s2.Y);
                CoefficientsVariableEven.Set(c);
                CoefficientsVariableOdd.Set(c);
            }
        }

        readonly EffectVectorVariable CoefficientsVariableEven;
        readonly EffectVectorVariable CoefficientsVariableOdd;

        protected bool IsDisposed;

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

            CoefficientsVariableEven = SolverEven.Variables["Coefficients"].AsVector();
            CoefficientsVariableOdd = SolverOdd.Variables["Coefficients"].AsVector();

            Scale = Vector2.One;
            Omega = 1f;
        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Debug.Assert(Buffer.Size == solution.Size);
            Debug.Assert(Buffer.Size == target.Size);

            SolverEven.ShaderResources["Target"].SetResource(target.SRView);
            SolverEven.ShaderResources["Solution"].SetResource(solution.SRView);
            SolverOdd.ShaderResources["Target"].SetResource(target.SRView);
            SolverOdd.ShaderResources["Solution"].SetResource(Buffer.SRView);

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
            if(!IsDisposed)
            {
                OmegaVariableEven.Dispose();
                OmegaVariableOdd.Dispose();
                CoefficientsVariableEven.Dispose();
                CoefficientsVariableOdd.Dispose();
                Buffer.Dispose();
                SolverEven.Dispose();
                SolverOdd.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

    }
}
