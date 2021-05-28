using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AEther
{
    public abstract class MovingFilter<T>
    {

        public abstract void Clear();

        public abstract T Filter(T x);

        public void FilterSpan(ReadOnlySpan<T> source, Span<T> destination, Func<T, T, T> op)
        {

            Debug.Assert(source.Length <= destination.Length);

            var buffer = ArrayPool<T>.Shared.Rent(source.Length);
            Array.Clear(buffer, 0, source.Length);

            Clear();
            for (var i = 0; i < source.Length; ++i)
            {
                buffer[i] = Filter(source[i]);
            }

            Clear();
            for (var i = source.Length - 1; 0 <= i; --i)
            {
                destination[i] = op(buffer[i], Filter(source[i]));
            }

        }

    }
}
