using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AEther.Benchmarks
{
    public class TimerBenchmark
    {

        [Benchmark]
        public void MovingMedianRef()
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
