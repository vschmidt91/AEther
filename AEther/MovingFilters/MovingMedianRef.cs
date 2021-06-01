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

        public MovingMedianRef(int windowSize, Comparison<T> comparison)
            : base(windowSize)
        {
            Size = 0;
            Position = 0;
            Comparer = Comparer<T>.Create(comparison);
            Buffer = new T[WindowSize];
        }

        public override void Clear()
        {
            Size = 0;
            Position = 0;
            Array.Clear(Buffer, 0, WindowSize);
        }

        public override T Filter(T value)
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

            
            return Buffer.Take(Size).OrderBy(v => v).Skip((Size - 1) / 2).First();

        }

    }
}
