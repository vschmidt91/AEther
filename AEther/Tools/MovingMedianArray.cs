using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingMedianArray<T> : ITimeFilter<T>, IFrequencyFilter<T>
    {

        public readonly int HalfWidth;
        public int Width => 2 * HalfWidth + 1;

        public readonly T[] Sorted;
        public readonly T[] Buffer;

        int Position;

        readonly Comparer<T> Comparer;

        public MovingMedianArray(int halfWidth, Comparer<T>? comparer = null)
        {

            Comparer = comparer ?? Comparer<T>.Default;
            HalfWidth = halfWidth;

            Sorted = new T[Width];
            Buffer = new T[Width];

            Clear();

        }

        public void Clear()
        {
            Array.Clear(Buffer, 0, Width);
            Array.Clear(Sorted, 0, Width);
        }

        public void Filter(ReadOnlySpan<T> src, Memory<T> dst)
        {

            Clear();

            for (int k = 0; k < HalfWidth; ++k)
            {
                Filter(src[k]);
            }

            for (int k = HalfWidth; k < src.Length; ++k)
            {
                dst.Span[k - HalfWidth] = Filter(src[k]);
            }

            for (int k = src.Length - HalfWidth; k < src.Length; ++k)
            {
                dst.Span[k] = src[^1];
            }

        }

        public T Filter(T value)
        {

            var oldValue = Buffer[Position];
            Buffer[Position] = value;

            Position++;
            if (Position == Buffer.Length)
                Position = 0;

            var comparison = Comparer.Compare(oldValue, value);
            if (comparison < 0)
            {
                var i = BinarySearchLeft(Sorted, oldValue);
                var j = BinarySearchRight(Sorted, value);
                Array.Copy(Sorted, i + 1, Sorted, i, j - i);
                Sorted[j] = value;
            }
            else if (0 < comparison)
            {
                var i = BinarySearchRight(Sorted, oldValue);
                var j = BinarySearchLeft(Sorted, value);
                Array.Copy(Sorted, j, Sorted, j + 1, i - j);
                Sorted[j] = value;
            }

            return Sorted[HalfWidth];

        }

        public int BinarySearchRight(T[] data, T value)
        {

            var l = 0;
            var r = data.Length;

            while(l < r)
            {
                var m = (l + r) / 2;
                if (Comparer.Compare(value, data[m]) < 0)
                    r = m;
                else
                    l = m + 1;
            }

            return r - 1;

        }

        public int BinarySearchLeft(T[] data, T value)
        {

            var l = 0;
            var r = data.Length;

            while (l < r)
            {
                var m = (l + r) / 2;
                if(Comparer.Compare(data[m], value) < 0)
                    l = m + 1;
                else
                    r = m;
            }

            return l;

        }

        public void ClearValue(T value)
        {
            Array.Fill(Buffer, value);
            Array.Fill(Sorted, value);
        }

    }
}
