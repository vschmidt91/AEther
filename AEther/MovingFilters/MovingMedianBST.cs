using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingMedianBST<T> : WindowedFilter<T>
    {

        public readonly Comparer<T> Comparer;
        public readonly T[] Buffer;
        public BinarySearchTree<T>? Tree;

        public int BufferPosition = 0;
        public int LeftCount = 0;
        public int RightCount = 0;

        public MovingMedianBST(int windowSize, Comparer<T> comparer)
            : base(windowSize)
        {
            Comparer = comparer;
            Tree = null;
            Buffer = new T[windowSize];
        }

        public override void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
            Tree = null;
            LeftCount = 0;
            RightCount = 0;
        }

        int Count => (Tree == null ? 0 : 1) + LeftCount + RightCount;

        T Median => Tree.Item;

        public override T Filter(T newItem)
        {
            if(WindowSize <= Count)
            {
                var oldItem = Buffer[BufferPosition];
                Remove(oldItem);
                Balance();
            }
            Buffer[BufferPosition] = newItem;
            BufferPosition = (BufferPosition + 1) % Buffer.Length;
            Insert(newItem);
            Balance();
            return Median;
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
            if(Tree == null)
            {
                Tree = new BinarySearchTree<T>(newItem, null, null, Comparer);
            }
            else
            {
                var comparison = Tree.Insert(newItem);
                if (comparison < 0)
                {
                    LeftCount++;
                }
                else
                {
                    RightCount++;
                }
            }
        }

        void Remove(T item)
        {
            var comparison = Comparer.Compare(item, Median);
            Tree = Tree.Remove(item);
            if (comparison <= 0)
            {
                LeftCount--;
            }
            else
            {
                RightCount--;
            }
        }

    }
}
