using System;
using System.Collections.Generic;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{
    public class Recorder : Listener
    {

        public Recorder()
            : base(new WaveIn())
        { }

    }
}
