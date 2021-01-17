using System;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AEther
{
    public abstract class SampleSource : IDisposable
    {

        public EventHandler<ReadOnlyMemory<byte>>? OnDataAvailable;
        public EventHandler<Exception?>? OnStopped;

        public abstract SampleFormat Format { get; }

        public abstract void Start();

        public virtual void Stop()
        {
            OnStopped?.Invoke(this, null);
        }

        public abstract void Dispose();

    }
}
