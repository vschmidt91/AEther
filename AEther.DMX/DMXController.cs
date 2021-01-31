using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

using AEther;

namespace AEther.DMX
{
    public class DMXController : IAsyncDisposable
    {

        const int Baudrate = 250000;

        readonly SerialPort Serial;
        readonly Task WriterTask;
        readonly CancellationTokenSource Cancel;
        readonly byte[] Frame;

        readonly MovingQuantileEstimator TransientFilter;

        public DMXController(string comPort)
        {
            TransientFilter = new MovingQuantileEstimator(.9f, .005f, 0f);
            var ports = SerialPort.GetPortNames();
            Serial = new SerialPort(comPort, Baudrate)
            {
                DataBits = 8,
                StopBits = StopBits.Two,
                Parity = Parity.None,
            };
            Serial.Open();
            Frame = new byte[513];
            Cancel = new();
            WriterTask = Task.Run(() => Write(Cancel.Token), Cancel.Token);
        }

        void Write(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                var lastIndex = Array.FindLastIndex(Frame, x => 0 < x);
                if (lastIndex != -1)
                {
                    Serial.Write(Frame, 0, lastIndex + 1);
                }
            }
        }

        public void Process(SampleEvent evt)
        {
            Array.Clear(Frame, 0, Frame.Length);
            var sum = new float[4];
            for(var i = 0; i < evt.SampleCount; ++i)
            {
                sum[i % sum.Length] += evt.Samples[i];
            }
            for (var i = 0; i < sum.Length; ++i)
            {
                sum[i] *= sum.Length / (float)evt.SampleCount;
            }
            var sinuoids = (byte)(5f * 255 * sum[0]);
            var transientFiltered = TransientFilter.Filter(sum[1]);
            var transients = transientFiltered < sum[1];
            var noise = (byte)(255 * sum[2]);
            var residual = (byte)(255 * sum[3]);
            Frame[1] = noise;
            Frame[2] = noise;
            Frame[3] = 0;
            Frame[4] = noise;
            Frame[5] = 0;
            Frame[6] = sinuoids;
            Frame[7] = 0;
            Frame[8] = sinuoids;
            Frame[9] = transients ? 20 : 200;
            //Frame[9] = (byte)(15f * 255 * sum[1]);
            Frame[10] = 0;
        }

        public async ValueTask DisposeAsync()
        {
            Cancel.Cancel();
            await WriterTask;
            Serial.Close();
            Serial.Dispose();
        }
    }
}
