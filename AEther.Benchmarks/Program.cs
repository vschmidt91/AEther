using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
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
            BenchmarkRunner.Run<FloatBenchmark>(config);
            //BenchmarkRunner.Run<WAVBenchmark>(config);
            //BenchmarkRunner.Run<MatrixBenchmark>(config);
            //BenchmarkRunner.Run<TimerBenchmark>(config);
#endif
        }
    }
}
