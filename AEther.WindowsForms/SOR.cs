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
    public class SOR : GraphicsComponent, IPoissonSolver, IDisposable
    {

        public int Iterations { get; set; } = 1;
        public float Omega { get; set; } = 1f;
        public Vector2 Scale { get; set; } = Vector2.One;

        readonly Texture2D Buffer;

        Shader Solver => Graphics.Shaders["poisson-sor.fx"];

        readonly EffectVectorVariable CoeffsVariable;
        readonly EffectScalarVariable OmegaVariable;
        readonly EffectScalarVariable UpdateEvenVariable;
        readonly EffectScalarVariable UpdateOddVariable;

        public SOR(Graphics graphics, int width, int height, Format format)
            : base(graphics)
        {
            Buffer = Graphics.CreateTexture(width, height, format);
            CoeffsVariable = Solver.Variables["Coefficients"].AsVector();
            OmegaVariable = Solver.Variables["Omega"].AsScalar();
            UpdateEvenVariable = Solver.Variables["UpdateEven"].AsScalar();
            UpdateOddVariable = Solver.Variables["UpdateOdd"].AsScalar();
        }

        public void Solve(Texture2D target, Texture2D solution)
        {

            Debug.Assert(Buffer.Size == solution.Size);
            Debug.Assert(Buffer.Size == target.Size);


            //float2 a = float2
            //(
            //    Solution.Sample(Point, IN.UV, int2(0, -1)) + Solution.Sample(Point, IN.UV, int2(0, +1)),
            //    Solution.Sample(Point, IN.UV, int2(-1, 0)) + Solution.Sample(Point, IN.UV, int2(+1, 0))
            //);
            //float p2 = ScaleSquared.x * ScaleSquared.y * -Target.Sample(Point, IN.UV) + dot(a, ScaleSquared);
            //p2 /= dot(ScaleSquared, 2);

            var s2 = Scale * Scale;
            var coeffs = .5f * new Vector3(s2.Y, s2.X, -s2.X * s2.Y) / (s2.X + s2.Y);
            CoeffsVariable.Set(coeffs);
            Solver.ShaderResources["Target"].SetResource(target.GetShaderResourceView());
            OmegaVariable.Set(Omega);

            for (var i = 0; i < Iterations; ++i)
            {

                UpdateEvenVariable.Set(true);
                UpdateOddVariable.Set(false);
                Solver.ShaderResources["Solution"].SetResource(solution.GetShaderResourceView());
                Graphics.SetFullscreenTarget(Buffer);
                Graphics.Draw(Solver);

                UpdateEvenVariable.Set(false);
                UpdateOddVariable.Set(true);
                Solver.ShaderResources["Solution"].SetResource(Buffer.GetShaderResourceView());
                Graphics.SetFullscreenTarget(solution);
                Graphics.Draw(Solver);

            }

        }

        public void Dispose()
        {
            Buffer.Dispose();
            CoeffsVariable.Dispose();
            OmegaVariable.Dispose();
            UpdateEvenVariable.Dispose();
            UpdateOddVariable.Dispose();
        }

    }
}
