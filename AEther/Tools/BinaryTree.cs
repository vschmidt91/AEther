using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther.Tools
{
    public class BinaryTree<T, X> where X : BinaryTree<T, X>
    {

        public readonly T Item;
        public readonly X? Left;
        public readonly X? Right;

        public BinaryTree(T item, X? left = null, X? right = null)
        {
            Item = item;
            Left = left;
            Right = right;
        }

    }
}
