﻿using System;
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

        public RingBuffer(int size, int? position = default)
            : this(new T[size], position)
        { }

        public RingBuffer(T[] data, int? position = default)
        {
            Data = data;
            Position = position ?? 0;
        }

        public T Add(T value)
        {
            var oldValue = Data[Position];
            Data[Position] = value;
            Interlocked.Increment(ref Position);
            Interlocked.CompareExchange(ref Position, 0, Data.Length);
            return oldValue;
        }

        public Memory<T> GetMemory(int length)
        {
            var count = Math.Min(Data.Length - Position, length);
            var memory = Data.AsMemory(Position, count);
            Position += count;
            if (Data.Length <= Position)
            {
                Position = 0;
            }
            return memory;
        }

    }
}
