using BenchmarkDotNet.Attributes;

using SharpDX;
using System.Numerics;

namespace AEther.Benchmarks
{
    public class MatrixBenchmark
    {

        const int Iterations = 1 << 20;

        [Benchmark]
        public void SharpDXBenchmark()
        {
            var M = Matrix.Identity;
            for (var i = 1; i < Iterations; i++)
            {
                var A = Matrix.Scaling(1f / i);
                var B = Matrix.RotationYawPitchRoll(i, i, i);
                var C = Matrix.Translation(new SharpDX.Vector3(i, i, i));
                M += A * B * C;
            }
        }

        [Benchmark]
        public void SystemNumericsBenchmark()
        {
            var M = Matrix4x4.Identity;
            for (var i = 1; i < Iterations; i++)
            {
                var A = Matrix4x4.CreateScale(1f / i);
                var B = Matrix4x4.CreateFromYawPitchRoll(i, i, i);
                var C = Matrix4x4.CreateTranslation(new System.Numerics.Vector3(i, i, i));
                M += A * B * C;
            }
        }

    }
}
