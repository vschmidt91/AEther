using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

using SharpDX;
using System.Numerics;

namespace AEther.Benchmarks
{
    public class MatrixBenchmark
    {

        static int Iterations = 1 << 20;

        [Benchmark]
        public void SharpDXBenchmark()
        {
            var M = SharpDX.Matrix.Identity;
            for(var i= 1; i < Iterations; i++)
            {
                var A = SharpDX.Matrix.Scaling(1f / i);
                var B = SharpDX.Matrix.RotationYawPitchRoll(i, i, i);
                var C = SharpDX.Matrix.Translation(new SharpDX.Vector3(i, i, i));
                M += A * B *C;
            }
        }

        [Benchmark]
        public void SystemNumericsBenchmark()
        {
            var M = System.Numerics.Matrix4x4.Identity;
            for (var i = 1; i < Iterations; i++)
            {
                var A = System.Numerics.Matrix4x4.CreateScale(1f / i);
                var B = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(i, i, i);
                var C = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(i, i, i));
                M += A * B * C;
            }
        }

    }
}
