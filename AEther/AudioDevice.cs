using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public abstract class AudioDevice : IDisposable
    {

        public EventHandler<ReadOnlyMemory<byte>>? OnDataAvailable;
        public EventHandler<Exception?>? OnStopped;

        public abstract SampleFormat Format { get; }

        public abstract void Dispose();

        public abstract void Start();

        public abstract void Stop();

    }
}
