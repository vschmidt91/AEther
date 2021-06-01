using System;
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

        public FixedHeap(int maxKey, Comparison<T> comparison)
            : base(ExtendComparer(comparison))
        {
            Indirection = new int[maxKey];
        }

        protected static Comparison<(T, int)> ExtendComparer(Comparison<T> comparer)
            => ((T, int) x, (T, int) y)
            => comparer(x.Item1, y.Item1);

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
