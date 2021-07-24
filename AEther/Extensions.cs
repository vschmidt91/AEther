using System;
using System.Buffers;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AEther
{

    public static class Extensions
    {

        public static List<IEnumerable<T>> Subsets<T>(this IEnumerable<T> values)
        {
            List<IEnumerable<T>> subsets = new();
            subsets.Add(Array.Empty<T>());
            foreach (var value in values)
            {
                var newSubsets = subsets.Select(l => l.Concat(new[] { value }));
                subsets.AddRange(newSubsets);
            }
            return subsets;
        }

        public static async IAsyncEnumerable<ReadResult> ReadAllAsync(this PipeReader reader, [EnumeratorCancellation]CancellationToken cancel = default)
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

        public static double Mix(this double x0, double x1, double q)
        {
            return x0 + q * (x1 - x0);
        }

        public static (IEnumerable<T1>, IEnumerable<T2>) Unzip<T1, T2>(this IEnumerable<(T1, T2)> items)
            => (items.Select(p => p.Item1), items.Select(p => p.Item2));

        public static void Swap<T>(this T[] values, int i, int j)
        {
            (values[i], values[j]) = (values[j], values[i]);
        }

        public static int NextMultipleOf(this int n, int k) => ((n - 1) / k + 1) * k;

        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> data, Func<T, IEnumerable<T>> unfold)
            => data.Concat(data.SelectMany(x => unfold(x).SelectDeep(unfold)));

        public static Vector3 Abs(this Vector3 t)
            => new()
            {
                X = Math.Abs(t.X),
                Y = Math.Abs(t.Y),
                Z = Math.Abs(t.Z),
            };

        public static Matrix4x4 ToDiagonalMatrix(this Vector4 v)
            => new(
                v.X, 0, 0, 0,
                0, v.Y, 0, 0,
                0, 0, v.Z, 0,
                0, 0, 0, v.W);

    }
}
