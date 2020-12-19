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
            BenchmarkRunner.Run<WAVBenchmark>();
        }
    }
}
