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
    public class Domain : IEnumerable<double>
    {

        public int Count => Values.Length;
        public int Resolution;
        public double Octaves => Count / Resolution;
        public double MinValue => Values[0];

        readonly double[] Values;

        public double this[int i] => Values[i];

        public Domain(double minValue, int resolution, int count)
        {
            Values = Enumerable.Range(0, count)
                .Select(i => minValue * Math.Pow(2, i / (double)resolution))
                .ToArray();
            Resolution = resolution;
        }

        public static Domain FromRange(double minValue, double maxValue, int resolution)
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

        public IEnumerator<double> GetEnumerator()
            => ((IEnumerable<double>)Values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<double>)Values).GetEnumerator();

    }
}
