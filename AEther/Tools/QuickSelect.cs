using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class QuickSelect<T>
        where T : struct
    {

        static GenericOperator<T, T, bool>.Operator LessThan
            => GenericOperator<T, T, bool>.LessThan;

        public static T Median(T[] values)
            => Rank(values, values.Length / 2, 0, values.Length - 1);

        public static T Rank(T[] values, int k, int l, int r)
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

        public static int Partition(T[] values, int l, int r, int p)
        {

            T pivot = values[p];
            values.Swap(p, r);

            int m = l;
            for(int i = l; i < r; ++i)
            {
                if (LessThan(values[i], pivot))
                    values.Swap(i, m++);
            }

            values.Swap(m, r);
            return m;

        }

    }
}
