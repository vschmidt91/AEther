﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace AEther
{
    public class FixedHeap<T> : Heap<(T, int)>
    {

        readonly int[] Indirection;

        public FixedHeap(int maxKey, IComparer<T> comparer)
            : base(ExtendComparer(comparer))
        {
            Indirection = new int[maxKey];
        }

        protected static IComparer<(T, int)> ExtendComparer(IComparer<T> comparer)
        {
            int Compare((T, int) x, (T, int) y)
            {
                var c = comparer.Compare(x.Item1, y.Item1);
                if (c == 0)
                {
                    return c;
                    //return x.Item2 - y.Item2;
                }
                else
                {
                    return c;
                }
            }
            return Comparer<(T, int)>.Create(Compare);
        }

        public override void Insert((T, int) item)
        {
            Indirection[item.Item2] = Size;
            base.Insert(item);
        }
        protected override void Swap(int i, int j)
        {
            base.Swap(i, j);
            Indirection[Items[i].Item2] = i;
            Indirection[Items[j].Item2] = j;
        }

        public void Delete(int key)
        {
            var i = Indirection[key];
            DeleteAt(i);
        }

    }

}
