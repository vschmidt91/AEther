using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace AEther.Benchmarks
{
    class Program
    {
        static void Main()
        {
            var config = ManualConfig
                .Create(DefaultConfig.Instance)
                .AddDiagnoser(MemoryDiagnoser.Default);
#if DEBUG
            new MatrixBenchmark().SystemNumericsBenchmark();
#else
            BenchmarkRunner.Run<MedianBenchmark>(config);
            //BenchmarkRunner.Run<WAVBenchmark>(config);
            //BenchmarkRunner.Run<MatrixBenchmark>(config);
            //BenchmarkRunner.Run<TimerBenchmark>(config);
#endif
        }
    }
}
