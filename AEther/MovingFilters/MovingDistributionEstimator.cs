using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public class MovingDistributionEstimator : MovingFilter<float>
    {

        public readonly MovingQuantileEstimator[] Quantiles;

        readonly float[] States;

        public MovingDistributionEstimator(int quantileCount, float mix)
        {
            Quantiles = Enumerable.Range(0, quantileCount)
                .Select(i => new MovingQuantileEstimator(i / (float)(quantileCount - 1), mix))
                .ToArray();
            States = new float[quantileCount];
        }

        public override void Clear()
        {
            foreach(var quantile in Quantiles)
            {
                quantile.Clear();
            }
        }

        public override float Filter(float x)
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
