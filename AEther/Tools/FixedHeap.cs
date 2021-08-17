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
                return comparer.Compare(x.Item1, y.Item1);
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
            (_, var ki) = Items[i];
            (_, var kj) = Items[j];
            Indirection[ki] = i;
            Indirection[kj] = j;
        }

        public void Delete(int key)
        {
            var i = Indirection[key];
            DeleteAt(i);
        }

    }

}
