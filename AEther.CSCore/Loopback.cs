using CSCore;
using CSCore.SoundIn;

namespace AEther.CSCore
{
    public class Loopback : Input
    {

        public Loopback()
            : base(CreateDevice())
        { }

        static WasapiLoopbackCapture CreateDevice()
        {
            var latency = 0;
            var format = new WaveFormat();
            var priority = ThreadPriority.Normal;
            return new WasapiLoopbackCapture(latency, format, priority);
        }


    }
}
