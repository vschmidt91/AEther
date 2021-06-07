using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public class MovingDistributionEstimator : MovingFilter<double>
    {

        public readonly MovingQuantileEstimator[] Quantiles;

        readonly double[] States;

        public MovingDistributionEstimator(double state, int quantileCount, double mix)
            : base(state)
        {
            Quantiles = Enumerable.Range(0, quantileCount)
                .Select(i => new MovingQuantileEstimator(state, i / (double)(quantileCount - 1), mix))
                .ToArray();
            States = new double[quantileCount];
        }

        public override void Clear(double state)
        {
            base.Clear(state);
            foreach(var quantile in Quantiles)
            {
                quantile.Clear(state);
            }
        }

        public override void Filter(double x)
        {
            for(int i = 0; i < Quantiles.Length; ++i)
            {
                Quantiles[i].Filter(x);
                States[i] = Quantiles[i].State;
            }
            for (int i = 0; i < Quantiles.Length; ++i)
            {
                if (x < States[i])
                    State = Quantiles[i].Quantile;
            }
        }

    }
}
