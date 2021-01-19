using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public abstract class Heap<T> : IEnumerable<T>
    {
        private const int GrowFactor = 2;
        private const int MinGrow = 1;

        T[] Items = Array.Empty<T>();
        private int Tail = 0;

        public int Count => Tail;
        public int Capacity => Items.Length;

        protected Comparer<T> Comparer { get; private set; }
        protected abstract bool Dominates(T x, T y);

        protected Heap() : this(Comparer<T>.Default)
        { }

        protected Heap(Comparer<T> comparer) : this(Enumerable.Empty<T>(), comparer)
        { }

        protected Heap(IEnumerable<T> collection)
            : this(collection, Comparer<T>.Default)
        { }

        protected Heap(IEnumerable<T> collection, Comparer<T> comparer)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

            //foreach (var item in collection)
            //{
            //    if (Count == Capacity)
            //        Grow();

            //    Items[Tail++] = item;
            //}
            Items = collection.Concat(Enumerable.Repeat<T>(default, 1)).ToArray();
            Tail = Items.Length - 1;

            for (var i = Parent(Tail - 1); 0 <= i; i--)
                BubbleDown(i);
        }

        public void Add(T item)
        {
            if (Count == Capacity)
                Grow();

            Items[Tail++] = item;
            BubbleUp(Tail - 1);
        }

        private void BubbleUp(int i)
        {
            if (i == 0)
                return;
            if (Dominates(Items[Parent(i)], Items[i]))
                return;
            Items.Swap(i, Parent(i));
            BubbleUp(Parent(i));
        }

        public T Remove(int i)
        {
            if (i < 0) throw new IndexOutOfRangeException(nameof(i));
            if (Count <= i) throw new InvalidOperationException("Heap is too small");
            T ret = Items[i];
            Tail--;
            Items.Swap(Tail, i);
            BubbleDown(i);
            return ret;
        }

        public void Remove(T item)
        {
            Remove(Array.IndexOf(Items, item));
        }

        public T GetMin()
        {
            if (Count == 0) throw new InvalidOperationException("Heap is empty");
            return Items[0];
        }

        public T ExtractDominating()
        {
            if (Count == 0) throw new InvalidOperationException("Heap is empty");
            T ret = Items[0];
            Tail--;
            Items.Swap(Tail, 0);
            BubbleDown(0);
            return ret;
        }

        private void BubbleDown(int i)
        {
            int dominatingNode = Dominating(i);
            if (dominatingNode == i) return;
            Items.Swap(i, dominatingNode);
            BubbleDown(dominatingNode);
        }

        private int Dominating(int i)
        {
            int dominatingNode = i;
            dominatingNode = GetDominating(YoungChild(i), dominatingNode);
            dominatingNode = GetDominating(OldChild(i), dominatingNode);
            return dominatingNode;
        }

        private int GetDominating(int newNode, int dominatingNode)
        {
            if (newNode < Tail && !Dominates(Items[dominatingNode], Items[newNode]))
                return newNode;
            else
                return dominatingNode;
        }

        private static int Parent(int i)
        {
            return (i + 1) / 2 - 1;
        }

        private static int YoungChild(int i)
        {
            return (i + 1) * 2 - 1;
        }

        private static int OldChild(int i)
        {
            return YoungChild(i) + 1;
        }

        private void Grow()
        {
            int newCapacity = Capacity * GrowFactor + MinGrow;
            var newHeap = new T[newCapacity];
            Array.Copy(Items, newHeap, Capacity);
            Items = newHeap;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Take(Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class MaxHeap<T> : Heap<T>
    {
        public MaxHeap()
            : this(Comparer<T>.Default)
        {
        }

        public MaxHeap(Comparer<T> comparer)
            : base(comparer)
        {
        }

        public MaxHeap(IEnumerable<T> collection, Comparer<T> comparer)
            : base(collection, comparer)
        {
        }

        public MaxHeap(IEnumerable<T> collection) : base(collection)
        {
        }

        protected override bool Dominates(T x, T y)
        {
            return Comparer.Compare(x, y) >= 0;
        }
    }

    public class MinHeap<T> : Heap<T>
    {
        public MinHeap()
            : this(Comparer<T>.Default)
        {
        }

        public MinHeap(Comparer<T> comparer)
            : base(comparer)
        {
        }

        public MinHeap(IEnumerable<T> collection) : base(collection)
        {
        }

        public MinHeap(IEnumerable<T> collection, Comparer<T> comparer)
            : base(collection, comparer)
        {
        }

        protected override bool Dominates(T x, T y)
        {
            return Comparer.Compare(x, y) <= 0;
        }
    }

}
