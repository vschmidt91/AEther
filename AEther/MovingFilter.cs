using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AEther
{
    public abstract class MovingFilter<T>
    {

        static ArrayPool<T> Pool => ArrayPool<T>.Shared;

        public T State { get; protected set; }

        public MovingFilter(T state)
        {
            State = state;
        }

        public virtual void Clear(T state)
        {
            State = state;
        }

        public abstract void Filter(T value);

        public void FilterSpan(ReadOnlySpan<T> source, Span<T> destination, Func<T, T, T> op)
        {

            Debug.Assert(source.Length <= destination.Length);

            var buffer = Pool.Rent(source.Length);

            Clear(buffer[0] = source[0]);
            for (var i = 1; i < source.Length; ++i)
            {
                Filter(source[i]);
                buffer[i] = State;
            }

            Clear(destination[^1] = buffer[^1]);
            for (var i = source.Length - 2; 0 <= i; --i)
            {
                Filter(source[i]);
                destination[i] = op(buffer[i], State);
            }

            Pool.Return(buffer);

        }

    }
}
