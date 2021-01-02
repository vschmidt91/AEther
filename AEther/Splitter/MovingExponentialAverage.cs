using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingExponentialAverage : ITimeFilter<float>, IFrequencyFilter<float>
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
            => new MovingExponentialAverage(2f / (1 + window));

        public void Clear()
        {
            State = 0f;
        }

        public float Filter(float newValue)
        {
            State += Mix * (newValue - State);
            return State;
        }

        public void Filter(ReadOnlySpan<float> src, Memory<float> dst)
        {

            State = src[0];
            for (int k = 0; k < src.Length; ++k)
            {
                dst.Span[k] = 0.5f * Filter(src[k]);
            }

            State = src[src.Length - 1];
            for (int k = src.Length - 1; 0 <= k; --k)
            {
                dst.Span[k] += 0.5f * Filter(src[k]);
            }

        }

    }
}
