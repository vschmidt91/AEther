using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AEther
{ 

    public class MovingMedianHeap<T> : ITimeFilter<T>, IFrequencyFilter<T>
    {

        readonly Heap<T> Below;
        readonly Heap<T> Above;

        int Position = 0;

        T Median;

        readonly int Size;
        readonly Comparer<T> Comparer;
        readonly bool[] Flags;

        public MovingMedianHeap(int size, Comparer<T>? comparer = null)
        {
            Size = size;
            Flags = new bool[Size];
            Comparer = comparer ?? Comparer<T>.Default;
            Below = new(size, Comparer<T>.Create((x, y) => -Comparer.Compare(x, y)));
            Above = new(size, Comparer);
        }

        public void Filter(ReadOnlySpan<T> src, Memory<T> dst)
        {


            Clear();
            for (int k = 0; k < src.Length; ++k)
            {
                dst.Span[k] = Filter(src[k]);
            }

            Clear();
            for (int k = src.Length - 1; 0 <= k; --k)
            {
                var x1 = dst.Span[k];
                var x2 = Filter(src[k]);
                dst.Span[k] = Comparer.Compare(x1, x2) < 0 ? x2 : x1;
            }

        }

        void Clear()
        {
            Below.Clear();
            Above.Clear();
        }

        public T Filter(T value)
        {

            if (Size <= Below.Size + Above.Size)
            {
                if (Flags[Position])
                {
                    Above.Delete(Position);
                }
                else
                {
                    Below.Delete(Position);
                }
            }

            if (Comparer.Compare(value, Median) < 0)
            {
                Below.Insert(Position, value);
                Flags[Position] = false;
            }
            else
            {
                Above.Insert(Position, value);
                Flags[Position] = true;
            }

            if (Below.Size + 1 <= Above.Size - 1)
            {
                var (key, item) = Above.DeleteRoot();
                Below.Insert(key, item);
                Flags[key] = false;
            }
            else if (Above.Size + 1 <= Below.Size - 1)
            {
                var (key, item) = Below.DeleteRoot();
                Above.Insert(key, item);
                Flags[key] = true;
            }

            Position = (Position + 1) % Size;

            if (Below.Size < Above.Size)
            {
                Median = Above.Root;
            }
            else
            {
                Median = Below.Root;
            }

            return Median;

        }

    }

}
