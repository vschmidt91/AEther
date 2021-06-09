using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class BinaryTree<T, X> where X : BinaryTree<T, X>
    {

        public T Item;
        public X? Left;
        public X? Right;

        public BinaryTree(T item, X? left = null, X? right = null)
        {
            Item = item;
            Left = left;
            Right = right;
        }

        public X Leftmost() => Left?.Leftmost() ?? (X)this;
        public X Rightmost() => Right?.Rightmost() ?? (X)this;

    }
}
