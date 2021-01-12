using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{

    public class MovingQuantile
    {

        public readonly float Quantile;
        public readonly float Eta;

        public float State { get; protected set; }

        public MovingQuantile(float quantile = .5f, float eta = 1f, float state = 0f)
        {
            Quantile = quantile;
            Eta = eta;
            State = state;
        }

        public float Filter(float x)
        {
            float delta = x - State;
            State += (Math.Sign(delta) + 2 * Quantile - 1) * Eta * Math.Abs(delta);
            return State;
        }

    }
}
