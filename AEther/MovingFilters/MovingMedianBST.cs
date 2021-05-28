using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingMedianBST<T> : MovingFilter<T>
    {

        public readonly Comparer<T> Comparer;
        public readonly T[] Buffer;
        public BinarySearchTree<T> Left;
        public BinarySearchTree<T> Right;

        public T Median { get; protected set; }

        public int BufferPosition = 0;
        public int LeftCount = 1;
        public int RightCount = 1;

        public MovingMedianBST(IEnumerable<T> items, Comparer<T>? comparer = null)
        {

            Comparer = comparer ?? Comparer<T>.Default;

            var threeItems = items.Take(3).ToArray();
            Array.Sort(threeItems, Comparer);
            Left = new BinarySearchTree<T>(threeItems[0], null, null, Comparer);
            Median = threeItems[1];
            Right = new BinarySearchTree<T>(threeItems[2], null, null, Comparer);

            Buffer = items.ToArray();
            foreach (var item in items.Skip(3))
            {
                Insert(item);
                Balance();
            }

        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override T Filter(T newItem)
        {
            var oldItem = Buffer[BufferPosition];
            Buffer[BufferPosition] = newItem;
            BufferPosition = (BufferPosition + 1) % Buffer.Length;
            Remove(oldItem);
            Insert(newItem);
            Balance();
            return Median;
        }

        void Balance()
        {
            if (2 < Math.Abs(LeftCount - RightCount))
            {
                throw new Exception();
            }
            else if (LeftCount + 1 <= RightCount - 1)
            {
                Left = Left.Insert(Median);
                LeftCount++;
                var (newRight, leftmost) = Right.RemoveLeftmost();
                if(newRight == null)
                {
                    throw new Exception();
                }
                Median = leftmost;
                Right = newRight;
                RightCount--;
            }
            else if (RightCount + 1 <= LeftCount - 1)
            {
                Right = Right.Insert(Median);
                RightCount++;
                var (newLeft, rightmost) = Left.RemoveRightmost();
                if (newLeft == null)
                {
                    throw new Exception();
                }
                Median = rightmost;
                Left = newLeft;
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
                if (LeftCount < RightCount)
                {
                    var (newRight, leftmost) = Right.RemoveLeftmost();
                    if(newRight == null)
                    {
                        throw new Exception();
                    }    
                    Right = newRight;
                    Median = leftmost;
                    RightCount--;
                }
                else
                {
                    var (newLeft, rightmost) = Left.RemoveRightmost();
                    if (newLeft == null)
                    {
                        throw new Exception();
                    }
                    Left = newLeft;
                    Median = rightmost;
                    LeftCount--;
                }
            }
            else if (comparison < 0)
            {
                var newLeft = Left.Remove(item);
                if(newLeft == null)
                {
                    throw new Exception();
                }
                Left = newLeft;
                LeftCount--;
            }
            else if (0 < comparison)
            {
                var newRight = Right.Remove(item);
                if (newRight == null)
                {
                    throw new Exception();
                }
                Right = newRight;
                RightCount--;
            }
        }

    }
}
