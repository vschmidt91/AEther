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

    public readonly struct PipeHandle
    {

        public readonly ReadOnlySequence<byte> Data;
        readonly PipeReader Reader;

        public PipeHandle(ReadOnlySequence<byte> data, PipeReader reader)
        {
            Data = data;
            Reader = reader;
        }

        public void AdvanceTo(SequencePosition consumed, SequencePosition observed)
        {
            Reader.AdvanceTo(consumed, observed);
        }

    }

    public static class Extensions
    {

        public static async IAsyncEnumerable<PipeHandle> ReadAllAsync(this PipeReader reader, [EnumeratorCancellation]CancellationToken cancel = default)
        {
            while (true)
            {
                var result = await reader.ReadAsync(cancel);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
                yield return new PipeHandle(result.Buffer, reader);
            }
        }

        public static float Clamp(this float x, float a, float b)
        {
            if (x < a)
                return a;
            else if (b < x)
                return b;
            else
                return x;
        }

        public static float Mix(this float x0, float x1, float q)
        {
            return x0 + q * (x1 - x0);
        }

        public static void Swap<T>(this T[] values, int i, int j)
        {
            T tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
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
