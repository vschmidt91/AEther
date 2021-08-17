using BenchmarkDotNet.Attributes;

namespace AEther.Benchmarks
{
    public class TimerBenchmark
    {

        [Benchmark]
        public static void MMTimer()
        {

            var timer = new MultimediaTimer()
            {
                Resolution = 0,
                Interval = 3,
            };

            var handle = new EventWaitHandle(false, EventResetMode.AutoReset);

            timer.Elapsed += (obj, evt) => handle.Set();
            timer.Start();

            handle.WaitOne();

        }

    }
}
