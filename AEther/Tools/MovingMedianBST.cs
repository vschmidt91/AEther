using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.Tools
{
    public class MovingMedianBST<T>
    {

        public readonly Comparer<T> Comparer;
        public readonly T[] Buffer;
        public T Median { get; protected set; }

        public int BufferPosition;
        public BinarySearchTree<T> Left;
        public BinarySearchTree<T> Right;
        public int LeftCount;
        public int RightCount;

        public MovingMedianBST(IEnumerable<T> items, Comparer<T> comparer = null)
        {

            Comparer = comparer ?? Comparer<T>.Default;

            var threeItems = items.Take(3).ToArray();
            Array.Sort(threeItems, Comparer);
            Left = new BinarySearchTree<T>(threeItems[0], null, null, Comparer);
            Median = threeItems[1];
            Right = new BinarySearchTree<T>(threeItems[2], null, null, Comparer);

            Buffer = items.ToArray();
            BufferPosition = Buffer.Length - 1;
            foreach (var item in items.Skip(3))
            {
                Insert(item);
                Balance();
            }

        }

        public T Rotate(T newItem)
        {
            var oldItem = Buffer[BufferPosition];
            Buffer[BufferPosition] = newItem;
            BufferPosition = (BufferPosition + 1) % Buffer.Length;
            Remove(oldItem);
            Balance();
            Insert(newItem);
            Balance();
        }

        void Balance()
        {
            if (1 < Math.Abs(LeftCount - RightCount))
            {
                throw new Exception();
            }
            else if (LeftCount + 1 <= RightCount - 1)
            {
                Left = Left.Insert(Median);
                LeftCount++;
                (Right, Median) = Right.RemoveLeftmost();
                RightCount--;
            }
            else if (RightCount + 1 <= LeftCount - 1)
            {
                Right = Right.Insert(Median);
                RightCount++;
                (Left, Median) = Left.RemoveRightmost();
                LeftCount--;
            }
        }

        void Insert(T newItem)
        {
            var comparison = Comparer.Compare(newItem, Median);
            if(comparison <= 0)
            {
                Left = Left.Insert(newItem);
                LeftCount++;
            }
            else if(0 < comparison)
            {
                Right = Right.Insert(newItem);
                RightCount++;
            }
        }

        void Remove(T item)
        {
            var comparison = Comparer.Compare(item, Median);
            if (comparison == 0)
            {
                if(LeftCount < RightCount)
                {
                    (Right, Median) = Right.RemoveLeftmost();
                    RightCount--;
                }
                else
                {
                    (Left, Median) = Left.RemoveRightmost();
                    LeftCount--;
                }
            }
            else if (comparison < 0)
            {
                Left = Left.Remove(item);
                LeftCount--;
            }
            else if (0 < comparison)
            {
                Right = Right.Remove(item);
                RightCount--;
            }
        }

    }
}
