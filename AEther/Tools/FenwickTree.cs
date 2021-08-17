namespace AEther
{
    public class FenwickTree<T>
        where T : struct, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
    {

        public int Size => Items.Length;

        readonly T[] Items;

        public FenwickTree(T[] items)
        {
            Items = items;
            for (int i = 0; i < Size; ++i)
            {
                int j = i + LSB(i + 1);
                if (j < Size)
                {
                    Items[j] += Items[i];
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
            for (; i > 0; i -= LSB(i))
            {
                sum += Items[i - 1];
            }
            return sum;
        }

        public T GetSum(int i, int j)
        {
            T sum = default;
            for (; j > i; j -= LSB(j))
            {
                sum += Items[j - 1];
            }
            for (; i > j; i -= LSB(i))
            {
                sum -= Items[i - 1];
            }
            return sum;
        }

        public T GetItem(int i) => GetSum(i, i + 1);

        public void AddItem(int i, T item)
        {
            for (; i < Size; i += LSB(i + 1))
            {
                Items[i] += item;
            }
        }

        public void SetItem(int i, T item)
        {
            AddItem(i, item - GetItem(i));
        }

        static int LSB(int n) => n & (-n);

    }
}
