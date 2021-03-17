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

    public class MovingMedianHeap<T> : ITimeFilter<T>
    {

        readonly Heap<T> Below;
        readonly Heap<T> Above;

        int Position = 0;

        public T Median => Below.Size < Above.Size ? Above.Root : Below.Root;

        readonly T[] Buffer;
        readonly Comparer<T> Comparer;

        public MovingMedianHeap(IEnumerable<T> items, Comparer<T>? comparer = null)
        {
            Comparer = comparer ?? Comparer<T>.Default;
            Buffer = items.ToArray();
            Below = new(items.Take(Buffer.Length / 2), Comparer<T>.Create((x, y) => -Comparer.Compare(x, y)));
            Above = new(items.Skip(Buffer.Length / 2), Comparer);
        }

        public T Filter(T value)
        {

            var oldValue = Buffer[Position];
            Buffer[Position] = value;
            Position = (Position + 1) % Buffer.Length;

            var median = Median;
            var comparison = Comparer.Compare(oldValue, median);
            if (comparison == 0)
            {
                if (Below.Size < Above.Size)
                {
                    Above.Delete(oldValue);
                }
                else
                {
                    Below.Delete(oldValue);
                }
            }
            else if (comparison < 0)
            {
                Below.Delete(oldValue);
            }
            else
            {
                Above.Delete(oldValue);
            }

            if (Comparer.Compare(value, median) < 0)
            {
                Below.Insert(value);
            }
            else
            {
                Above.Insert(value);
            }

            if (Below.Size + 1 <= Above.Size - 1)
            {
                Below.Insert(Above.DeleteRoot());
            }

            if (Above.Size + 1 <= Below.Size - 1)
            {
                Above.Insert(Below.DeleteRoot());
            }

            return Median;

        }

    }

}
