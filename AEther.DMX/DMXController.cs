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

        public DMXController(string comPort)
        {
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
                var length = Array.FindLastIndex(Frame, x => 0 < x) switch
                {
                    -1 => Frame.Length,
                    var i => i + 1,
                };
                Serial.Write(Frame, 0, length);
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
                sum[i] *= sum.Length / evt.SampleCount;
            }
            var sinuoids = (byte)(255 * sum[0]);
            var transients = (byte)(255 * sum[1]);
            var noise = (byte)(255 * sum[3]);
            var residual = (byte)(255 * sum[4]);
            Frame[1] = transients;
            Frame[2] = transients;
            Frame[3] = 0;
            Frame[4] = noise;
            Frame[5] = 0;
            Frame[6] = sinuoids;
            Frame[7] = 0;
            Frame[8] = sinuoids;
            Frame[9] = transients;
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
