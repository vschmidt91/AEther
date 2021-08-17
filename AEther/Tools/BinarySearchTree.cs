using System.Collections;

namespace AEther
{
    public class BinarySearchTree<T> : BinaryTree<T, BinarySearchTree<T>>
    {

        readonly Comparer<T> Comparer;

        public BinarySearchTree(T item, BinarySearchTree<T>? left, BinarySearchTree<T>? right, Comparer<T> comparer)
            : base(item, left, right)
        {
            Comparer = comparer;
        }

        public int Insert(T newItem)
        {
            var comparison = Comparer.Compare(newItem, Item);
            if (comparison < 0)
            {
                if (Left == null)
                {
                    Left = new BinarySearchTree<T>(newItem, null, null, Comparer);
                }
                else
                {
                    Left.Insert(newItem);
                }
            }
            else
            {
                if (Right == null)
                {
                    Right = new BinarySearchTree<T>(newItem, null, null, Comparer);
                }
                else
                {
                    Right.Insert(newItem);
                }
            }
            return comparison;
        }

        public BinarySearchTree<T>? Remove(T oldItem)
        {
            var comparison = Comparer.Compare(oldItem, Item);
            if (comparison == 0)
            {
                if (Left is null)
                {
                    return Right;
                }
                else if (Right is null)
                {
                    return Left;
                }
                else
                {
                    (Left, Item) = Left?.RemoveRightmost() ?? throw new KeyNotFoundException();
                    //(Right, Item) = Right?.RemoveLeftmost() ?? throw new KeyNotFoundException();
                    return this;
                }
            }
            else if (comparison < 0)
            {
                if (Left is null)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    Left = Left.Remove(oldItem);
                    return this;
                }
            }
            else
            {
                if (Right is null)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    Right = Right.Remove(oldItem);
                    return this;
                }
            }
        }

        public void RotateRight()
        {
            var item = Item;
            (Left, Item) = Left?.RemoveRightmost() ?? throw new KeyNotFoundException();
            Insert(item);
        }

        public void RotateLeft()
        {
            var item = Item;
            (Right, Item) = Right?.RemoveLeftmost() ?? throw new KeyNotFoundException();
            Insert(item);
        }

        public (BinarySearchTree<T>?, T) RemoveRightmost()
        {
            if (Right is null)
            {
                return (Left, Item);
            }
            else
            {
                var (newRight, rightmostItem) = Right.RemoveRightmost();
                Right = newRight;
                return (this, rightmostItem);
            }
        }

        public (BinarySearchTree<T>?, T) RemoveLeftmost()
        {
            if (Left is null)
            {
                return (Right, Item);
            }
            else
            {
                var (newLeft, leftmostItem) = Left.RemoveLeftmost();
                Left = newLeft;
                return (this, leftmostItem);
            }
        }

    }
}
