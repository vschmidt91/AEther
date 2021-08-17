using System.Collections;

namespace AEther
{
    public class MovingMedianBST<T> : WindowedFilter<T>
    {

        public readonly Comparer<T> Comparer;
        public readonly T[] Buffer;
        public BinarySearchTree<T> Tree;

        public int Position = 0;
        public int LeftCount = 0;
        public int RightCount = 0;

        public MovingMedianBST(T state, int windowSize, Comparer<T> comparer)
            : base(state, windowSize)
        {
            Comparer = comparer;
            Tree = new BinarySearchTree<T>(state, null, null, Comparer);
            Buffer = new T[windowSize];
        }

        public override void Clear(T state)
        {
            base.Clear(state);
            Position = 0;
            Array.Fill(Buffer, state, 0, Buffer.Length);
            Tree = new BinarySearchTree<T>(state, null, null, Comparer);
            LeftCount = 0;
            RightCount = 0;
        }

        int Count => 1 + LeftCount + RightCount;

        public override void Filter(T newItem)
        {
            if (WindowSize <= Count)
            {
                var oldItem = Buffer[Position];
                Remove(oldItem);
                Balance();
            }
            Buffer[Position] = newItem;
            Position = (Position + 1) % Buffer.Length;
            Insert(newItem);
            Balance();
            State = Tree.Item;
        }

        void Balance()
        {
            while (LeftCount + 1 < RightCount)
            {
                Tree.RotateLeft();
                RightCount--;
                LeftCount++;
            }
            while (RightCount < LeftCount)
            {
                Tree.RotateRight();
                LeftCount--;
                RightCount++;
            }
        }

        void Insert(T newItem)
        {
            if (Tree.Insert(newItem) < 0)
            {
                LeftCount++;
            }
            else
            {
                RightCount++;
            }
        }

        void Remove(T item)
        {
            if (Comparer.Compare(item, State) <= 0)
            {
                LeftCount--;
            }
            else
            {
                RightCount--;
            }
            if (Tree.Remove(item) is BinarySearchTree<T> t)
            {
                Tree = t;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

    }
}
