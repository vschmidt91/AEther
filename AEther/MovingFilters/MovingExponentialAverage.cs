using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingExponentialAverage : MovingFilter<double>
    {

        public double Average => State;

        readonly double Mix;

        double State;

        public MovingExponentialAverage(double mix)
        {
            Mix = mix;
            State = 0f;
        }

        public static MovingExponentialAverage FromWindow(int window)
            => new(2f / (1 + window));

        public override void Clear()
        {
            State = 0f;
        }

        public override double Filter(double newValue)
        {
            State += Mix * (newValue - State);
            return State;
        }

    }
}
