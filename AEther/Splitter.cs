using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class Splitter
    {

        readonly Domain Domain;

        readonly MovingFilter<double> FrequencyTransients;
        readonly MovingFilter<double>[] TimeSinuoids;
        readonly MovingFilter<double> FrequencySinuoids;
        readonly MovingFilter<double>[] TimeTransients;

        readonly double[] Buffer1;
        readonly double[] Buffer2;
        readonly double[] Buffer3;
        readonly double[] Buffer4;
        readonly double[] KeyWeights;

        public Splitter(Domain domain, SessionOptions options)
        {

            Domain = domain;

            Buffer1 = new double[Domain.Count];
            Buffer2 = new double[Domain.Count];
            Buffer3 = new double[Domain.Count];
            Buffer4 = new double[Domain.Count];
            KeyWeights = new double[Domain.Resolution];

            FrequencyTransients = CreateFilter((int)(options.TransientWidth * domain.Resolution));
            FrequencySinuoids = CreateFilter((int)(options.SinuoidWidth * domain.Resolution));

            TimeSinuoids = domain
                .Select((f, k) => CreateFilter((int)(options.SinuoidLength * options.TimeResolution)))
                .ToArray();
            TimeTransients = domain
                .Select((f, k) => CreateFilter((int)(options.TransientLength * options.TimeResolution)))
                .ToArray();

        }

        static WindowedFilter<double> CreateFilter(int windowSize)
            => new MovingMedianArray<double>(0.0, windowSize, Comparer<double>.Default);


        public void Process(ReadOnlyMemory<double> input, Memory<double> output)
        {

            static double combine(double x, double y)
            {
                return .5 * (x + y);
            }

            var src = input.Span;
            var dst = output.Span;

            FrequencyTransients.FilterSpan(src, Buffer1, combine);
            for (int k = 0; k < Domain.Count; ++k)
            {
                TimeSinuoids[k].Filter(src[k]);
                Buffer2[k] = TimeSinuoids[k].State;

                TimeTransients[k].Filter(Buffer1[k]);
                Buffer3[k] = TimeTransients[k].State;
            }
            FrequencySinuoids.FilterSpan(Buffer2, Buffer4, combine);

            Array.Clear(KeyWeights, 0, KeyWeights.Length);
            var noiseFloor = Enumerable.Range(0, input.Length).Sum(k => input.Span[k]) / input.Length;

            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                var sinuoids = Math.Max(0, Buffer2[k] - Buffer4[k]);
                var transients = Math.Max(0, Buffer1[k] - Buffer3[k]);
                var noise = Math.Max(0, src[k] - noiseFloor);

                KeyWeights[k % KeyWeights.Length] += sinuoids * Domain.Resolution / Domain.Count;

                sinuoids = Math.Max(0, 1 + 1.2 * (sinuoids - 1));
                transients = Math.Max(0, 1 + 1.2 * (transients - 1));

                y[0] = sinuoids;
                y[1] = transients;
                y[2] = 1 + .1 * src[k];
                y[3] = 0;

            }

            //var key = Array.IndexOf(KeyWeights, KeyWeights.Max()) / (float)KeyWeights.Length;

            //for (int k = 0; k < Domain.Count; ++k)
            //{
            //    var y = dst.Slice(4 * k, 4);
            //    y[2] = KeyWeights[k % KeyWeights.Length];
            //}

        }

    }
}
