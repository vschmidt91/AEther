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

        static int Parent(int i) => (i - 1) / 2;
        static int LeftChild(int i) => 2 * i + 1;
        static int RightChild(int i) => 2 * i + 2;

        public int Size { get; protected set; } = 0;
        public int Capacity => Items.Length;

        T[] Items = Array.Empty<T>();
        int[] Keys = Array.Empty<int>();

        readonly int[] Indirection;
        readonly Comparer<T> Comparer;

        public Heap(int maxKey, Comparer<T> comparer)
        {
            Comparer = comparer;
            Indirection = new int[maxKey];
        }

        public void Insert(int key, T item)
        {
            if (Size == Capacity)
            {
                Grow();
            }
            Items[Size] = item;
            Keys[Size] = key;
            Indirection[key] = Size;
            Size++;
            BubbleUp(Size - 1);
        }
        void Swap(int i, int j)
        {
            Items.Swap(i, j);
            Keys.Swap(i, j);
            Indirection[Keys[i]] = i;
            Indirection[Keys[j]] = j;
        }

        void BubbleUp(int i)
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

        public (int, T) DeleteAt(int i)
        {
            while(0 < i)
            {
                var p = Parent(i);
                Swap(i, p);
                i = p;
            }
            return DeleteRoot();
        }

        public (int, T) Delete(int key)
        {
            var i = Indirection[key];
            return DeleteAt(i);
        }

        public T Root => Items[0];

        public (int, T) DeleteRoot()
        {
            var key = Keys[0];
            var value = Items[0];
            Size--;
            Swap(Size, 0);
            BubbleDown(0);
            return (key, value);
        }

        public void Clear()
        {
            Size = 0;
        }

        void BubbleDown(int i)
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

        void Grow()
        {

            int newCapacity = 1 + 2 * Capacity;

            var newItems = new T[newCapacity];
            Array.Copy(Items, newItems, Capacity);
            Items = newItems;

            var newKeys = new int[newCapacity];
            Array.Copy(Keys, newKeys, Keys.Length);
            Keys = newKeys;

        }

    }

}
