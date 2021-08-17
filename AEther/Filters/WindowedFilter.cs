namespace AEther
{
    public abstract class WindowedFilter<T> : MovingFilter<T>
    {

        public readonly int WindowSize;

        public WindowedFilter(T state, int windowSize)
            : base(state)
        {
            WindowSize = windowSize;
        }

    }
}
