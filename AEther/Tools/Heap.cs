using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public class Heap<T>
    {

        static int Parent(int i) => (i - 1) / 2;
        static int LeftChild(int i) => 2 * i + 1;
        static int RightChild(int i) => 2 * i + 2;

        const int GrowFactor = 2;
        const int MinGrow = 1;

        public int Size { get; protected set; }
        public int Capacity => Items.Length;

        T[] Items = Array.Empty<T>();
        readonly Comparer<T> Comparer;

        public Heap(IEnumerable<T> collection, Comparer<T> comparer)
        {

            Comparer = comparer;
            Items = collection.Concat(collection.Take(1)).ToArray();
            Size = Capacity - 1;

            for (var i = Parent(Size - 1); 0 <= i; i--)
            {
                BubbleDown(i);
            }

        }

        public void Insert(T item)
        {
            if (Size == Capacity)
            {
                Grow();
            }
            Items[Size++] = item;
            BubbleUp(Size - 1);
        }

        public void Check()
        {
            for(var i = 1; i < Size - 1; ++i)
            {
                if (Comparer.Compare(Items[i], Items[Parent(i)]) < 0)
                    throw new Exception();
            }
        }

        void BubbleUp(int i)
        {
            if (i == 0)
            {
                return;
            }
            var p = Parent(i);
            if (Comparer.Compare(Items[i], Items[p]) < 0)
            {
                Items.Swap(i, p);
                BubbleUp(p);
            }
        }

        public T DeleteAt(int i)
        {
            while(0 < i)
            {
                var p = Parent(i);
                Items.Swap(i, p);
                i = p;
            }
            return DeleteRoot();
        }

        public void Delete(T item)
        {
            var i = Array.IndexOf(Items, item);
            if (0 <= i)
            {
                DeleteAt(i);
            }
        }

        public T Root => Items[0];

        public T DeleteRoot()
        {
            var root = Root;
            Items.Swap(--Size, 0);
            BubbleDown(0);
            return root;
        }

        void BubbleDown(int i)
        {
            var smallest = i;
            smallest = Smaller(smallest, LeftChild(i));
            smallest = Smaller(smallest, RightChild(i));
            if (smallest != i)
            {
                Items.Swap(i, smallest);
                BubbleDown(smallest);
            }
        }

        int Smaller(int i, int j)
        {
            if (Size <= j)
            {
                return i;
            }
            else if (Comparer.Compare(Items[j], Items[i]) < 0)
            {
                return j;
            }
            else
            {
                return i;
            }
        }

        void Grow()
        {
            int newCapacity = Capacity * GrowFactor + MinGrow;
            var newItems = new T[newCapacity];
            Array.Copy(Items, newItems, Capacity);
            Items = newItems;
        }

    }

}
