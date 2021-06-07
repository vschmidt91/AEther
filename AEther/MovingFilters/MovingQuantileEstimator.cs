using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingQuantileEstimator : MovingFilter<double>
    {

        public readonly double Quantile;
        public readonly double Mix;

        public MovingQuantileEstimator(double state, double quantile, double mix)
            : base(state)
        {
            Quantile = quantile;
            Mix = mix;
        }

        public override void Filter(double value)
        {
            //var mix = Math.Max(Mix, Math.Abs(State));
            State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            //State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            if (double.IsNaN(value))
                throw new Exception();
        }

    }
}
