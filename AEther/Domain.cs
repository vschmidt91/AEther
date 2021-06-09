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
    public record Domain
    (
        double MinValue,
        int Resolution,
        int Length
    )
    : IEnumerable<double>
    {

        public double Octaves => Length / Resolution;

        public static Domain FromRange(double minValue, double maxValue, int resolution)
        {
            int length = (int)Math.Round(resolution * Math.Log(maxValue / minValue, 2));
            return new Domain(minValue, resolution, length);
        }

        public IEnumerator<double> GetEnumerator()
            => Enumerable.Range(0, Length)
            .Select(i => MinValue * Math.Pow(2, i / (double)Resolution))
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<double>)this).GetEnumerator();

    }
}
