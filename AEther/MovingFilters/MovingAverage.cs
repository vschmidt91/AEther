using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingAverage : MovingFilter<float>
    {

        readonly float[] Buffer;
        readonly int HalfSize;

        float Average;
        int Position;

        public MovingAverage(int halfSize)
        {
            HalfSize = halfSize;
            Average = 0f;
            Buffer = new float[2 * HalfSize + 1];
            Position = 0;
        }

        public override void Clear()
        {
            Average = 0f;
            Array.Clear(Buffer, 0, Buffer.Length);
            Position = 0;
        }

        public override float Filter(float newValue)
        {
            var oldValue = Buffer[Position];
            Buffer[Position] = newValue;
            Average += (newValue - oldValue) / Buffer.Length;
            Position++;
            if(Position == Buffer.Length)
            {
                Position = 0;
            }
            return Average;
        }

    }
}
