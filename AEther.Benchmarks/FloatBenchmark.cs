using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;

namespace AEther.Benchmarks
{
    public class FloatBenchmark
    {

        const int Length = 1 << 20;
        const int Seed = 12345;

        readonly byte[] Source = new byte[Length];
        readonly double[] Destination = new double[Length];

        [Benchmark]
        public void BitConverter()
        {
            Run(CopyWithBitConverter);
        }

        [Benchmark]
        public void BlockCopy()
        {
            Run(CopyWithBlockCopy);
        }

        public void Run(Action copy)
        {
            var rng = new Random(Seed);
            rng.NextBytes(Source);
            copy();
        }

        void CopyWithBitConverter()
        {
            for (int i = 0; i < Destination.Length; ++i)
            {
                Destination[i] = System.BitConverter.ToSingle(Source, sizeof(float) * i);
            }
        }

        void CopyWithBlockCopy()
        {
            var buffer = ArrayPool<float>.Shared.Rent(Destination.Length);
            Buffer.BlockCopy(Source, 0, buffer, 0, Source.Length);
            for (int i = 0; i < Destination.Length; ++i)
            {
                Destination[i] = buffer[i];
            }
            ArrayPool<float>.Shared.Return(buffer);
        }

    }
}
