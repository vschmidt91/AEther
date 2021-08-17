namespace AEther
{
    public abstract class SampleSource : IDisposable
    {

        public EventHandler<ReadOnlyMemory<byte>>? DataAvailable;
        public EventHandler<Exception?>? Stopped;

        public abstract SampleFormat Format { get; }
        public abstract void Start();
        public abstract void Stop();
        public abstract void Dispose();

    }
}
