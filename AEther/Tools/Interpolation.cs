using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AEther
{
    public class Interpolation<T>
        : IEnumerable<KeyValuePair<float, T>>
        where T : struct
    {

        private readonly SortedDictionary<float, T> Values;

        public Interpolation(IDictionary<float, T> values = null)
        {
            if (values is null)
            {
                Values = new SortedDictionary<float, T>();
            }
            else
            {
                Values = new SortedDictionary<float, T>(values);
            }
        }

        static T Interpolate(T x, T y, float t)
        {
            T tx = GenericOperator<T, float, T>.Multiply(x, 1 - t);
            T ty = GenericOperator<T, float, T>.Multiply(y, t);
            return GenericOperator<T, T, T>.Add(tx, ty);
        }

        public T this[float t]
        {
            get
            {
                KeyValuePair<float, T>? kvp0 = null;
                KeyValuePair<float, T>? kvp1 = null;
                foreach (var kvp in Values)
                {
                    if (kvp.Key < t)
                    {
                        kvp0 = kvp;
                    }
                    else if (kvp.Key >= t)
                    {
                        kvp1 = kvp;
                        break;
                    }
                }
                if (kvp0.HasValue && kvp1.HasValue)
                {
                    float s = (t - kvp0.Value.Key) / (kvp1.Value.Key - kvp0.Value.Key);
                    return Interpolate(kvp0.Value.Value, kvp1.Value.Value, s);
                }
                else if (kvp0.HasValue)
                    return kvp0.Value.Value;
                else if (kvp1.HasValue)
                    return kvp1.Value.Value;
                else
                    return default;
            }
            set
            {
                Values[t] = value;
            }
        }

        public void Add(float t, T x)
            => this[t] = x;

        public T Get(float t)
            => this[t];

        public IEnumerator<KeyValuePair<float, T>> GetEnumerator()
            => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

    }
}
