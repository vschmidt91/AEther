using CSCore.SoundIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CSCore;

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

        public override string ToString()
            => "Loopback";


    }
}
