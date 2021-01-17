using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AEther
{
    public class Domain : IEnumerable<float>
    {

        public int Count => Values.Length;
        public int Resolution;
        public float Octaves => Count / Resolution;
        public float MinValue => Values[0];

        readonly float[] Values;

        public float this[int i] => Values[i];

        public Domain(float minValue, int resolution, int count)
        {
            Values = Enumerable.Range(0, count)
                .Select(i => minValue * (float)Math.Pow(2, i / (double)resolution))
                .ToArray();
            Resolution = resolution;
        }

        public static Domain FromRange(float minValue, float maxValue, int resolution)
        {
            int count = (int)Math.Round(resolution * Math.Log(maxValue / minValue, 2));
            return new Domain(minValue, resolution, count);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Domain other)
                return base.Equals(obj);

            return other.MinValue == MinValue
                && other.Count == Count
                && other.Resolution == Resolution;

        }

        public override int GetHashCode()
            => base.GetHashCode()
                ^ (123456789 * Resolution.GetHashCode())
                ^ (234567891 * Count.GetHashCode())
                ^ (345678912 * MinValue.GetHashCode());

        public IEnumerator<float> GetEnumerator()
            => ((IEnumerable<float>)Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<float>)Values).GetEnumerator();

    }
}
