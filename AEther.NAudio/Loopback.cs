
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
