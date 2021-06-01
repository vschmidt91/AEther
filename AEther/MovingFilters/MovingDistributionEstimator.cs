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

        public MovingDistributionEstimator(int quantileCount, float mix)
        {
            Quantiles = Enumerable.Range(0, quantileCount)
                .Select(i => new MovingQuantileEstimator(i / (float)(quantileCount - 1), mix))
                .ToArray();
            States = new double[quantileCount];
        }

        public override void Clear()
        {
            foreach(var quantile in Quantiles)
            {
                quantile.Clear();
            }
        }

        public override double Filter(double x)
        {
            for(int i = 0; i < Quantiles.Length; ++i)
            {
                States[i] = Quantiles[i].Filter(x);
            }
            for (int i = 0; i < Quantiles.Length; ++i)
            {
                if (x < States[i])
                    return Quantiles[i].Quantile;
            }
            return 1f;
        }

    }
}
