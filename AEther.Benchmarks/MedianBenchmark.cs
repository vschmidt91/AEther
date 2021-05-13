using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AEther.Benchmarks
{
    public class MedianBenchmark
    {

        readonly int Width = 1 << 8;
        readonly int Length = 1 << 14;
        readonly int Seed = 0;

        [Benchmark]
        public void MedianArray()
        {
            var median = new MovingMedianArray<float>(Width);
            var rng = new Random(Seed);
            for (var i = 0; i < 1 + 2 * Width; ++i)
            {
                median.Filter(0f);
            }
            for (var i = 0; i < Length; ++i)
            {
                var x = (float)rng.NextDouble();
                median.Filter(x);
            }
        }

        [Benchmark]
        public void MedianBST()
        {
            var median = new MovingMedianBST<float>(Enumerable.Repeat(0f, 1 + 2 * Width));
            var rng = new Random(Seed);
            for (var i = 0; i < 1 + 2 * Width; ++i)
            {
                median.Filter(0f);
            }
            for (var i = 0; i < Length; ++i)
            {
                var x = (float)rng.NextDouble();
                median.Filter(x);
            }
        }

        [Benchmark]
        public void MedianHeap()
        {
            var median = new MovingMedianHeap<float>(1 + 2 * Width);
            var rng = new Random(Seed);
            for (var i = 0; i < 1 + 2 * Width; ++i)
            {
                median.Filter(0f);
            }
            for (var i = 0; i < Length; ++i)
            {
                var x = (float)rng.NextDouble();
                median.Filter(x);
            }
        }

    }
}
