using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public record QuickSelect<T>
    (
        Comparison<T> Comparison
    )
    where T : struct
    {

        public T Median(T[] values)
            => Rank(values, values.Length / 2, 0, values.Length - 1);

        public T Rank(T[] values, int k, int l, int r)
        {
            if (l == r)
                return values[l];
            int p = Partition(values, l, r, l);
            if (k == p)
                return values[p];
            else if (k < p)
                return Rank(values, k, l, p - 1);
            else
                return Rank(values, k, p + 1, r);
        }

        public int Partition(T[] values, int l, int r, int p)
        {

            T pivot = values[p];
            values.Swap(p, r);

            int m = l;
            for(int i = l; i < r; ++i)
            {
                if (Comparison(values[i], pivot) < 0)
                    values.Swap(i, m++);
            }

            values.Swap(m, r);
            return m;

        }

    }
}
