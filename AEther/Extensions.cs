using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace AEther
{

    public static class Extensions
    {

        public static IEnumerable<IEnumerable<T>> Subsets<T>(this IEnumerable<T> values)
        {
            if (values.Any())
            {
                var head = values.Take(1);
                foreach (var tail in values.Skip(1).Subsets())
                {
                    yield return tail;
                    yield return head.Concat(tail);
                }
            }
            else
            {
                yield return Enumerable.Empty<T>();
            }
        }

        public static async IAsyncEnumerable<ReadResult> ReadAllAsync(this PipeReader reader, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            while (true)
            {
                cancel.ThrowIfCancellationRequested();
                ReadResult result = await reader.ReadAsync(cancel);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
                yield return result;
            }
        }

        public static T Clamp<T>(this T x, T a, T b)
            where T : IComparable<T>
        {
            if (x.CompareTo(a) < 0)
                return a;
            else if (0 < x.CompareTo(b))
                return b;
            else
                return x;
        }

        public static void Swap<T>(this T[] values, int i, int j)
        {
            (values[i], values[j]) = (values[j], values[i]);
        }

    }
}
