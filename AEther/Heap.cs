using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public class Heap<T>
        where T : struct
    {

        public int Size => Data.Length;

        readonly T[] Data;
        readonly int[] Positions;
        readonly Comparer<T> Comparer;

        int Index;

        public Heap(int size, Comparer<T> comparer)
            : this(Enumerable.Repeat(default(T), size), comparer)
        { }

        public Heap(IEnumerable<T> data, Comparer<T> comparer)
        {
            Data = data.ToArray();
            Positions = Enumerable.Range(0, Size).ToArray();
            Index = 0;
            Comparer = comparer;
        }

        public T Root => Data[0];

        public T Rotate(T newValue)
        {
            T oldValue = Replace(Positions[Index], newValue);
            Index = (Index + 1) % Size;
            return oldValue;
        }

        int Left(int i) => 2 * i + 1;
        int Right(int i) => 2 * i + 2;
        int Parent(int i) => (i - 1) / 2;

        void Swap(int i, int j)
        {
            Data.Swap(i, j);
            Positions.Swap(i, j);
        }

        void Heapify(int i)
        {

            var m = i;
            var l = Left(i);
            var r = Right(i);

            if (l < Size && Comparer.Compare(Data[l], Data[m]) < 0)
            {
                m = l;
            }
            if (r < Size && Comparer.Compare(Data[r], Data[m]) < 0)
            {
                m = r;
            }
            if (m != i)
            {
                Swap(i, m);
                Heapify(m);
            }

        }

        void DecreaseKey(int i, T value)
        {
            Data[i] = value;
            var p = Parent(i);
            while(0 < i && Comparer.Compare(Data[i], Data[p]) < 0)
            {
                Swap(i, p);
                i = p;
                p = Parent(i);
            }
        }

        T Replace(int i, T newValue)
        {
            T oldValue = Data[i];
            Data[i] = newValue;
            var p = Parent(i);
            if (i == 0)
            {
                Heapify(i);
            }
            else if (Comparer.Compare(Data[p], Data[i]) < 0)
            {
                Heapify(i);
            }
            else
            {
                DecreaseKey(i, newValue);
            }
            return oldValue;
        }

    }

}
