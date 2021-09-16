using System;

namespace AEther
{
    public class MovingAverage : MovingFilter<double>
    {

        readonly double[] Buffer;
        readonly int HalfSize;

        int Position;

        public MovingAverage(double state, int halfSize)
            : base(state)
        {
            HalfSize = halfSize;
            Buffer = new double[2 * HalfSize + 1];
            Position = 0;
        }

        public override void Clear(double state)
        {
            base.Clear(state);
            Array.Fill(Buffer, state, 0, Buffer.Length);
            Position = 0;
        }

        public override void Filter(double newValue)
        {
            var oldValue = Buffer[Position];
            Buffer[Position] = newValue;
            State += (newValue - oldValue) / Buffer.Length;
            Position++;
            if (Position == Buffer.Length)
            {
                Position = 0;
            }
        }

    }
}
