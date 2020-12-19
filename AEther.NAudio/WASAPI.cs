using System;
using System.Collections.Generic;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{

    public class WASAPI : Listener
    {

        public WASAPI()
            : base(new WasapiLoopbackCapture())
        { }

    }
}
