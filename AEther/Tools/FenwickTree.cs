using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class FenwickTree<T>
        where T : struct
    {

        static GenericOperator<T, T, T>.Operator Add
            => GenericOperator<T, T, T>.Add;

        static GenericOperator<T, T, T>.Operator Subtract
            => GenericOperator<T, T, T>.Subtract;

        public int Size => Items.Length;

        readonly T[] Items;

        public FenwickTree(T[] items)
        {
            Items = items;
            for(int i = 0; i < Size; ++i)
            {
                int j = i + LSB(i + 1);
                if (j < Size)
                {
                    Items[j] = Add(Items[j], Items[i]);
                }
            }
        }

        public FenwickTree(int size)
        {
            Items = new T[size];
        }

        public T GetSum(int i)
        {
            T sum = default;
            for(; i > 0; i -= LSB(i))
            {
                sum = Add(sum, Items[i - 1]);
            }
            return sum;
        }

        public T GetSum(int i, int j)
        {
            T sum = default;
            for (; j > i; j -= LSB(j))
            {
                sum = Add(sum, Items[j - 1]);
            }
            for (; i > j; i -= LSB(i))
            {
                sum = Subtract(sum, Items[i - 1]);
            }
            return sum;
        }

        public T GetItem(int i) => GetSum(i, i + 1);

        public void AddItem(int i, T item)
        {
            for(; i < Size; i += LSB(i + 1))
            {
                Items[i] = Add(Items[i], item);
            }
        }

        public void SetItem(int i, T item)
        {
            AddItem(i, Subtract(item, GetItem(i)));
        }

        static int LSB(int n) => n & (-n);

    }
}
