﻿namespace AEther
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

        public override string ToString()
            => $"{Item} ({Left?.ToString() ?? "()"} {Right?.ToString() ?? "()"})";

    }
}
