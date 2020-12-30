using System;
using System.Collections.Generic;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{

    public class WASAPI : Input
    {

        public WASAPI()
            : base(new WasapiLoopbackCapture())
        { }

    }
}
