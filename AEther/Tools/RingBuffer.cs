using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AEther
{
    public class CyclicBuffer<T> : IEnumerable<T>
    {

        public int Size => Data.Length;

        readonly T[] Data;
        int Position;

        public CyclicBuffer(int size)
        {
            Data = new T[size];
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Rotate(T value)
        {
            T oldValue;
            //lock (Data)
            {
                oldValue = Data[Position];
                Data[Position] = value;
            }
            if (++Position == Data.Length)
                Position = 0;
            return oldValue;
        }

        public void Clear()
        {
            lock (Data)
            {
                Position = 0;
                Array.Clear(Data, 0, Size);
            }
        }

        public IEnumerator<T> GetEnumerator()
            => ((IEnumerable<T>)Data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

    }
}
