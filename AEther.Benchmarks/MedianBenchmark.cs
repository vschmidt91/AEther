using BenchmarkDotNet.Attributes;

namespace AEther.Benchmarks
{
    public class MedianBenchmark
    {

        readonly int Width = 1 << 14;
        readonly int Length = 1 << 15;
        readonly int Seed = 0;
        readonly double State = 0.0;
        readonly Comparer<double> Comparer = Comparer<double>.Default;

        [Benchmark]
        public void MovingMedianRef()
            => Median(new MovingMedianArray<double>(State, Width, Comparer), Length, Seed);

        [Benchmark]
        public void MovingMedianArray()
            => Median(new MovingMedianArray<double>(State, Width, Comparer), Length, Seed);

        [Benchmark]
        public void MovingMedianBST()
            => Median(new MovingMedianBST<double>(State, Width, Comparer), Length, Seed);

        [Benchmark]
        public void MovingMedianHeap()
            => Median(new MovingMedianHeap<double>(State, Width, Comparer), Length, Seed);

        public static void Median(MovingFilter<double> filter, int length, int seed = 0)
        {
            var rng = new Random(seed);
            for (var i = 0; i < length; ++i)
            {
                var x = rng.NextDouble();
                filter.Filter(x);
            }
        }

    }
}
