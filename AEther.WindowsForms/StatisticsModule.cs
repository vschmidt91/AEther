using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class StatisticsModule
    {

        readonly TimeSpan UpdateInterval;
        readonly Form Form;
        readonly Stopwatch FrameTimer;
        readonly Stopwatch UpdateTimer;

        TimeSpan Latency;

        public StatisticsModule(Form form, double interval = 1)
        {
            UpdateInterval = TimeSpan.FromSeconds(interval);
            Form = form;
            FrameTimer = Stopwatch.StartNew();
            UpdateTimer = Stopwatch.StartNew();
            Latency = TimeSpan.Zero;
        }

        public void Update()
        {

            var frameTime = FrameTimer.Elapsed;
            FrameTimer.Restart();

            if (UpdateInterval < UpdateTimer.Elapsed)
            {
                Form.Text = $"Latency: {Math.Round(Latency.TotalMilliseconds, 0)} ms, Draw Time: {Math.Round(frameTime.TotalMilliseconds, 0)} ms";
                UpdateTimer.Restart();
                Latency = TimeSpan.Zero;
            }

        }

        public void Process(SampleEvent<double> evt)
        {
            var latency = DateTime.Now - evt.Time;
            if(Latency < latency)
            {
                Latency = latency;
            }
        }

    }
}
