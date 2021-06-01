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
        readonly DMXChannel[] Channels;
        readonly Domain Domain;

        readonly double[] KeyWeights;
        int Key;

        readonly MovingQuantileEstimator SinuoidFilter;
        readonly MovingQuantileEstimator TransientFilter;

        public double SinuoidThreshold;
        public double TransientThreshold;

        public DMXController(string comPort, Domain domain)
        {
            Channels = EuroliteLEDMultiFXBar.Create();
            Domain = domain;

            SinuoidFilter = new MovingQuantileEstimator(.95f, .003f);
            TransientFilter = new MovingQuantileEstimator(.95f, .003f);

            KeyWeights = new double[Domain.Resolution];
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

        public void Process(SampleEvent<double> evt)
        {


            Array.Clear(KeyWeights, 0, KeyWeights.Length);
            for (int k = 0; k < Domain.Count; ++k)
            {
                KeyWeights[k % KeyWeights.Length] += evt.Samples[4 * k];
            }
            var newWeight = KeyWeights.Max();
            var newKey = Array.IndexOf(KeyWeights, newWeight);
            //if(newKey != Key && 1.3 * KeyWeights[Key] < newWeight)
            {
                Key = newKey;
            }

            var sum = new double[4];
            var scale = sum.Length / (double)evt.SampleCount;
            for (var i = 0; i < evt.SampleCount; ++i)
            {
                sum[i % sum.Length] += scale * evt.Samples[i];
            }

            //var sinuoid = sum[0] / SinuoidThreshold;
            //var transients = sum[1] / TransientThreshold;

            var sinuoid = sum[0] / SinuoidFilter.Filter(sum[0]);
            var transients = sum[1] / TransientFilter.Filter(sum[1]);

            Array.Clear(Frame, 0, Frame.Length);
            for (var i = 0; i < Channels.Length; ++i)
            {
                var channel = Channels[i];
                if(channel is DMXChannelDiscrete discrete)
                {
                    if (sinuoid < 1)
                    {
                        //discrete.Index = 0;
                    }
                    else
                    {
                        if(1 < discrete.Count)
                        {
                            discrete.Index = 1 + (Key % (discrete.Count - 1));
                        }
                    }
                }
                else if(channel is DMXChannelContinuous continuous)
                {
                    continuous.Value = transients.Clamp(0, 1);
                }
                Frame[i + 1] = channel.ByteValue;
            }

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
