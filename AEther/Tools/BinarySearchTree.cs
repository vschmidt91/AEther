using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class BinarySearchTree<T> : BinaryTree<T, BinarySearchTree<T>>
    {

        readonly Comparison<T> Comparison;

        public BinarySearchTree(T item, BinarySearchTree<T>? left, BinarySearchTree<T>? right, Comparison<T> comparison)
            : base(item, left, right)
        {
            Comparison = comparison;
        }

        public BinarySearchTree<T> Insert(T newItem)
        {
            var comparison = Comparison(newItem, Item);
            if(comparison <= 0)
            {
                if(Left == null)
                {
                    return new BinarySearchTree<T>(Item, new BinarySearchTree<T>(newItem, null, null, Comparison), Right, Comparison);
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left.Insert(newItem), Right, Comparison);
                }
            }
            else
            {
                if (Right == null)
                {
                    return new BinarySearchTree<T>(Item, Left, new BinarySearchTree<T>(newItem, null, null, Comparison), Comparison);
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left, Right.Insert(newItem), Comparison);
                }
            }
        }

        public BinarySearchTree<T>? Remove(T oldItem)
        {
            var comparison = Comparison(oldItem, Item);
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
                    var (newRight, successorItem) = Right.RemoveLeftmost();
                    return new BinarySearchTree<T>(successorItem, Left, newRight, Comparison);
                }
            }
            else if(comparison < 0)
            {
                if(Left == null)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left.Remove(oldItem), Right, Comparison);
                }
            }
            else
            {
                if (Right == null)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return new BinarySearchTree<T>(Item, Left, Right.Remove(oldItem), Comparison);
                }
            }
        }

        public (BinarySearchTree<T>?, T) RemoveRightmost()
        {
            if(Right == null)
            {
                return (Left, Item);
            }
            else
            {
                var (newRight, rightmostItem) = Right.RemoveRightmost();
                return (new BinarySearchTree<T>(Item, Left, newRight, Comparison), rightmostItem);
            }
        }

        public (BinarySearchTree<T>?, T) RemoveLeftmost()
        {
            if (Left == null)
            {
                return (Right, Item);
            }
            else
            {
                var (newLeft, leftmostItem) = Left.RemoveLeftmost();
                return (new BinarySearchTree<T>(Item, newLeft, Right, Comparison), leftmostItem);
            }
        }

    }
}
