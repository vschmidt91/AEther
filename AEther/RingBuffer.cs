using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;

namespace AEther
{
    public class RingBuffer<T>
    {

        public int Size => Data.Length;

        public T this[int i] => Data[Position - i + (Position < i ? Size : 0)];

        public T[] Data;
        public int Position;

        public RingBuffer(T[] data, int? position = default)
        {
            Data = data;
            Position = position ?? 0;
        }

        public static RingBuffer<T> Create(int size, int? position = default)
        {
            return new RingBuffer<T>(new T[size], position);
        }

        public T Add(T value)
        {
            var oldValue = Data[Position];
            Data[Position] = value;
            Interlocked.Increment(ref Position);
            Interlocked.CompareExchange(ref Position, 0, Data.Length);
            return oldValue;
        }

        public void Advance(ReadOnlyMemory<T> values)
        {
            var dst = Data.AsMemory(Position, values.Length);
            values.CopyTo(dst);
            Position += values.Length;
            if (Data.Length <= Position)
            {
                Position = 0;
            }
        }

        public (ReadOnlyMemory<T>, ReadOnlyMemory<T>) Add(ReadOnlyMemory<T> values)
        {
            var count = Math.Min(Data.Length - Position, values.Length);
            var src = values.Slice(0, count);
            var dst = Data.AsMemory(Position, count);
            return (src, dst);
        }

    }
}
