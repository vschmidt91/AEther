using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingExponentialAverage : MovingFilter<float>
    {

        public float Average => State;

        readonly float Mix;

        float State;

        public MovingExponentialAverage(float mix)
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

        public override float Filter(float newValue)
        {
            State += Mix * (newValue - State);
            return State;
        }

    }
}
