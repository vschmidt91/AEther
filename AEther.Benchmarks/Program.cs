using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace AEther.Benchmarks
{
    class Program
    {
        static void Main()
        {
#if DEBUG
            new MatrixBenchmark().SystemNumericsBenchmark();
#else
            //BenchmarkRunner.Run<WAVBenchmark>();
            BenchmarkRunner.Run<MatrixBenchmark>();
            //BenchmarkRunner.Run<TimerBenchmark>();
#endif
        }
    }
}
