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
    public class MovingMedianArray<T> : WindowedFilter<T>
    {

        public int Size;
        int Position;

        public readonly T[] Sorted;
        public readonly T[] Buffer;
        readonly Comparer<T> Comparer;

        public MovingMedianArray(T state, int windowSize, Comparer<T> comparer)
            : base(state, windowSize)
        {
            Sorted = new T[WindowSize];
            Buffer = new T[WindowSize];
            Comparer = comparer;
            Clear(state);
        }

        public override void Clear(T state)
        {
            base.Clear(state);
            Size = 1;
            Position = 0;
            Array.Fill(Buffer, state, 0, WindowSize);
            Array.Fill(Sorted, state, 0, WindowSize);
        }

        public override void Filter(T value)
        {

            if (Size < WindowSize)
            {

                var i = BinarySearchLeft(Sorted, value);
                Array.Copy(Sorted, i, Sorted, i + 1, Size - i);
                Sorted[i] = value;
                Size++;

            }
            else
            {

                var oldValue = Buffer[Position];

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

            }

            Buffer[Position] = value;

            Position++;
            if (Position == WindowSize)
                Position = 0;

            State = Sorted[(Size - 1) / 2];

        }

        int BinarySearchRight(T[] data, T value)
        {

            var l = 0;
            var r = Size;

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

        int BinarySearchLeft(T[] data, T value)
        {

            var l = 0;
            var r = Size;

            while (l < r)
            {
                var m = (l + r) / 2;
                if (Comparer.Compare(data[m], value) < 0)
                        l = m + 1;
                else
                    r = m;
            }

            return l;

        }

    }
}
