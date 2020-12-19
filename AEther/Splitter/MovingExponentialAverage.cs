using System;
using System.Collections.Generic;
using System.Text;

namespace AEther
{
    public class MovingExponentialAverage : ITimeFilter<float>, IFrequencyFilter<float>
    {

        readonly float Mix;

        float Average;

        public MovingExponentialAverage(float mix)
        {
            Mix = mix;
            Average = 0f;
        }

        public void Clear()
        {
            Average = 0f;
        }

        public float Filter(float newValue)
        {
            Average += Mix * (newValue - Average);
            return Average;
        }

        public void Filter(ReadOnlySpan<float> src, Memory<float> dst)
        {

            Average = src[0];
            for (int k = 0; k < src.Length; ++k)
            {
                dst.Span[k] = 0.5f * Filter(src[k]);
            }

            Average = src[src.Length - 1];
            for (int k = src.Length - 1; 0 <= k; --k)
            {
                dst.Span[k] += 0.5f * Filter(src[k]);
            }

        }

    }
}
