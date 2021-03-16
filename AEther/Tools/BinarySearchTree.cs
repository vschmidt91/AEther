using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.Tools
{
    public class BinarySearchTree<T> : BinaryTree<T, BinarySearchTree<T>>
    {

        readonly Comparer<T> Comparer;

        public BinarySearchTree(T item, BinarySearchTree<T>? left = null, BinarySearchTree<T>? right = null, Comparer<T>? comparer = null)
            : base(item, left, right)
        {
            Comparer = comparer ?? Comparer<T>.Default;
        }

        public BinarySearchTree<T> Insert(T newItem)
        {
            var comparison = Comparer.Compare(newItem, Item);
            if(comparison <= 0)
            {
                if(Left == null)
                {
                    return new BinarySearchTree<T>(Item, new BinarySearchTree<T>(newItem), Right, Comparer);
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left.Insert(newItem), Right, Comparer);
                }
            }
            else
            {
                if (Right == null)
                {
                    return new BinarySearchTree<T>(Item, Left, new BinarySearchTree<T>(newItem), Comparer);
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left, Right.Insert(newItem), Comparer);
                }
            }
        }

        public BinarySearchTree<T> Remove(T oldItem)
        {
            var comparison = Comparer.Compare(oldItem, Item);
            if(comparison == 0)
            {
                if (Left == null)
                {
                    return Right;
                }
                else if (Right == null)
                {
                    return Left;
                }
                else
                {
                    //var newLeft, predecessorItem = Left.RemoveRightmost();
                    //return new BinarySearchTree<T>(predecessorItem, newLeft, Right, Comparer);
                    var newRight, successorItem = Right.RemoveLeftmost();
                    return new BinarySearchTree<T>(successorItem, Left, newRight, Comparer);
                }
            }
            else if(comparison < 0)
            {
                return new BinarySearchTree<T>(Item, Left.Remove(oldItem), Right, Comparer);
            }
            else
            {
                return new BinarySearchTree<T>(Item, Left, Right.Remove(oldItem), Comparer);
            }
        }

        public (BinarySearchTree<T>, T) RemoveRightmost()
        {
            if(Right == null)
            {
                return (Left, Item);
            }
            else
            {
                var newRight, rightmostItem = Right.RemoveRightmost();
                return (new BinarySearchTree<T>(Item, Left, newRight, Comparer), rightmostItem);
            }
        }

        public (BinarySearchTree<T>, T) RemoveLeftmost()
        {
            if (Left == null)
            {
                return (Right, Item);
            }
            else
            {
                var newLeft, leftmostItem = Left.RemoveLeftmost();
                return (new BinarySearchTree<T>(Item, newLeft, Right, Comparer), leftmostItem);
            }
        }

    }
}
