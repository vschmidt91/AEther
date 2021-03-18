using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingQuantileEstimator : IFrequencyFilter<float>
    {

        public readonly float Quantile;
        public readonly float Mix;

        float State;

        public MovingQuantileEstimator(float quantile, float mix, float? initialState = default)
        {

            Quantile = quantile;
            Mix = mix;

            State = initialState ?? 0;

        }

        public void Clear()
        {
            State = 0;
        }

        public void Filter(ReadOnlySpan<float> src, Memory<float> dst)
        {

            State = src[0];
            for (int k = 0; k < src.Length; ++k)
            {
                dst.Span[k] = 0.5f * Filter(src[k]);
            }

            State = src[^1];
            for (int k = src.Length - 1; 0 <= k; --k)
            {
                dst.Span[k] += 0.5f * Filter(src[k]);
            }

        }

        public float Filter(float value)
        {
            //var mix = Math.Max(Mix, Math.Abs(State));
            State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            //State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            if (float.IsNaN(value))
                throw new Exception();
            return State;
        }

    }
}
