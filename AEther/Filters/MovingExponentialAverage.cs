using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingExponentialAverage : MovingFilter<double>
    {

        public double Average => State;

        readonly double Mix;

        public MovingExponentialAverage(double state, double mix)
            : base(state)
        {
            Mix = mix;
        }

        public static MovingExponentialAverage FromWindow(double state, int window)
            => new(state, 2 / (1.0 + window));

        public override void Filter(double newValue)
        {
            State += Mix * (newValue - State);
        }

    }
}
