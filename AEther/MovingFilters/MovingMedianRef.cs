using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingMedianRef<T> : WindowedFilter<T>
    {

        public int Size;
        int Position;

        public readonly T[] Buffer;
        readonly Comparer<T> Comparer;

        public MovingMedianRef(T state, int windowSize, Comparer<T> comparer)
            : base(state, windowSize)
        {
            Comparer = comparer;
            Buffer = new T[WindowSize];
            Clear(state);
        }

        public override void Clear(T state)
        {
            base.Clear(state);
            Size = 1;
            Position = 0;
            Array.Fill(Buffer, state, 0, WindowSize);
        }

        public override void Filter(T value)
        {

            if(Size < WindowSize)
            {
                Size++;
            }

            Buffer[Position] = value;

            Position++;
            if(Position == WindowSize)
            {
                Position = 0;
            }

            State = Buffer.Take(Size).OrderBy(v => v, Comparer).Skip((Size - 1) / 2).First();

        }

    }
}
