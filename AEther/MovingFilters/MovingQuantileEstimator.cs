using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingQuantileEstimator : MovingFilter<float>
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

        public override void Clear()
        {
            State = 0;
        }

        public override float Filter(float value)
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
