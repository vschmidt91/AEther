using System;
using System.Linq;
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
        readonly MovingQuantileEstimator SinuoidFilter;
        readonly MovingQuantileEstimator TransientFilter;

        readonly DMXChannel[] Channels = EuroliteLEDMultiFXBar.Create();

        public int Key;

        readonly Domain Domain;
        readonly float[] KeyWeights;

        public DMXController(string comPort, Domain domain, int timeResolution)
        {
            Domain = domain;
            KeyWeights = new float[Domain.Resolution];
            SinuoidFilter = new MovingQuantileEstimator(.95f, 2f / timeResolution);
            TransientFilter = new MovingQuantileEstimator(.95f, 2f / timeResolution);
            var ports = SerialPort.GetPortNames();
            Serial = new SerialPort(comPort, Baudrate)
            {
                DataBits = 8,
                StopBits = StopBits.Two,
                Parity = Parity.None,
            };
            Serial.Open();
            Frame = new byte[1 + Channels.Length];
            Cancel = new();
            WriterTask = Task.Run(() => Write(Cancel.Token), Cancel.Token);
        }

        void Write(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                Serial.Write(Frame, 0, 1 + Channels.Length);
            }
        }

        public void Process(SampleEvent evt)
        {


            Array.Clear(KeyWeights, 0, KeyWeights.Length);
            for (int k = 0; k < Domain.Count; ++k)
            {
                var y = evt.Samples.AsSpan(4 * k, 4);
                KeyWeights[k % KeyWeights.Length] += y[0] * Domain.Resolution / Domain.Count;
            }
            var key = Array.IndexOf(KeyWeights, KeyWeights.Max());

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

            //var sinuoid = sum[0] / SinuoidFilter.Filter(sum[0]);
            //var transients = sum[1] / TransientFilter.Filter(sum[1]);
            var sinuoid = sum[0] / 0.1f;
            var transients = sum[1] / 0.35f;
            //var sinuoid = sum[0] / Math.Max(0.1f, SinuoidFilter.Filter(sum[0]));
            //var transients = sum[1] / Math.Max(0.3f, TransientFilter.Filter(sum[1]));

            for (var i = 0; i < Channels.Length; ++i)
            {
                var channel = Channels[i];
                if(channel is DMXChannelDiscrete discrete)
                {
                    if(sinuoid < 1)
                    {
                        discrete.Index = 0;
                    }
                    else if (1 < discrete.Count)
                    {
                        discrete.Index = 1 + (key % (discrete.Count - 1));
                    }
                }
                else if(channel is DMXChannelContinuous continuous)
                {
                    continuous.Value = transients.Clamp(0, 1);
                }
                Frame[i + 1] = channel.ByteValue;
            }

            //var sinuoids = (byte)(255 * sum[0]);

            //byte transients = 0;
            //if(.2 < sum[1])
            //{
            //    transients = 20;
            //}
            //else if (.1 < sum[1])
            //{
            //    transients = 200;
            //}

            ////var transients = (byte)(sum[1] / transientFiltered switch
            ////{
            ////    < 1f => 20,
            ////    _ => 200,
            ////});
            ////var transients = (byte)(transientFiltered < sum[1] ? 20 : 200);

            //var noise = (byte)(255 * sum[2]);
            //var residual = (byte)(255 * sum[3]);
            //Frame[1] = noise;
            //Frame[2] = noise;
            //Frame[3] = 0;
            //Frame[4] = noise;
            //Frame[5] = 0;
            //Frame[6] = sinuoids;
            //Frame[7] = 0;
            //Frame[8] = sinuoids;
            //Frame[9] = transients;
            ////Frame[9] = (byte)(15f * 255 * sum[1]);
            //Frame[10] = 0;
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
