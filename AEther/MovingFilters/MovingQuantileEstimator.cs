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

        double State;

        public MovingQuantileEstimator(double quantile, double mix, double? initialState = default)
        {

            Quantile = quantile;
            Mix = mix;

            State = initialState ?? 0;

        }

        public override void Clear()
        {
            State = 0;
        }

        public override double Filter(double value)
        {
            //var mix = Math.Max(Mix, Math.Abs(State));
            State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            //State += Mix * (Math.Sign(value - State) + 2 * Quantile - 1);
            if (double.IsNaN(value))
                throw new Exception();
            return State;
        }

    }
}
