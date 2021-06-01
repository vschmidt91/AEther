using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace AEther
{
    public class Heap<T>
    {

        protected static int Parent(int i) => (i - 1) / 2;
        protected static int LeftChild(int i) => 2 * i + 1;
        protected static int RightChild(int i) => 2 * i + 2;

        public int Size { get; protected set; } = 0;
        public int Capacity => Items.Length;

        protected T[] Items = Array.Empty<T>();
        protected readonly IComparer<T> Comparer;

        public Heap(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public virtual void Insert(T item)
        {
            if (Size == Capacity)
            {
                Grow();
            }
            Items[Size] = item;
            Size++;
            BubbleUp(Size - 1);
        }

        public void DeleteAt(int i)
        {
            while(0 < i)
            {
                var p = Parent(i);
                Swap(i, p);
                i = p;
            }
            DeleteRoot();
        }

        public T Root => Items[0];

        public void DeleteRoot()
        {
            Size--;
            Swap(Size, 0);
            BubbleDown(0);
        }

        public void Clear()
        {
            Size = 0;
        }

        protected virtual void Swap(int i, int j)
        {
            Items.Swap(i, j);
        }

        protected void BubbleUp(int i)
        {
            if (0 < i)
            {
                var p = Parent(i);
                if (Comparer.Compare(Items[i], Items[p]) < 0)
                {
                    Swap(i, p);
                    BubbleUp(p);
                }
            }
        }

        protected void BubbleDown(int i)
        {

            var smallest = i;

            var l = LeftChild(i);
            if (l < Size && Comparer.Compare(Items[l], Items[smallest]) < 0)
                smallest = l;

            var r = RightChild(i);
            if (r < Size && Comparer.Compare(Items[r], Items[smallest]) < 0)
                smallest = r;

            if (smallest != i)
            {
                Swap(i, smallest);
                BubbleDown(smallest);
            }

        }

        protected void Grow()
        {

            int newCapacity = 1 + 2 * Capacity;

            var newItems = new T[newCapacity];
            Array.Copy(Items, newItems, Capacity);
            Items = newItems;

        }

    }

}
