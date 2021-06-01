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

        readonly int Width = 1 << 12;
        readonly int Length = 1 << 16;
        readonly int Seed = 0;
        readonly Comparison<float> Comparison = Comparer<float>.Default.Compare;

        [Benchmark]
        public void MovingMedianRef()
            => Median(new MovingMedianArray<float>(Width, Comparison), Length, Seed);

        [Benchmark]
        public void MovingMedianArray()
            => Median(new MovingMedianArray<float>(Width, Comparison), Length, Seed);

        //[Benchmark]
        //public void MovingMedianBST()
        //    => Median(new MovingMedianBST<float>(Width, Comparison), Length, Seed);

        [Benchmark]
        public void MovingMedianHeap()
            => Median(new MovingMedianHeap<float>(Width, Comparison), Length, Seed);

        [Benchmark]
        public void SleepTest()
            => System.Threading.Thread.SpinWait(1000);

        public void Median(MovingFilter<float> filter, int length, int seed = 0)
        {
            var rng = new Random(seed);
            for (var i = 0; i < length; ++i)
            {
                var x = (float)rng.NextDouble();
                filter.Filter(x);
            }
        }

    }
}
