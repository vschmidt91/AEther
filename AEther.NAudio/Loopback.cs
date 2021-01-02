using System;
using System.Collections.Generic;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{

    public class Loopback : Input
    {

        public Loopback()
            : base(new WasapiLoopbackCapture())
        { }

    }
}
