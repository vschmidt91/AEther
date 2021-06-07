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

            TimeSinuoids = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter((int)(options.SinuoidLength * options.TimeResolution)))
                .ToArray();
            TimeTransients = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter((int)(options.TransientLength * options.TimeResolution)))
                .ToArray();

        }

        static WindowedFilter<double> CreateFilter(int windowSize)
            => new MovingMedianArray<double>(windowSize, Comparer<double>.Default);

        static double Rolloff(double x) => 1 - Math.Exp(-x);

        public void Process(ReadOnlyMemory<double> input, Memory<double> output)
        {

            var src = input.Span;
            var dst = output.Span;

            FrequencyTransients.FilterSpan(src, Buffer1, (x, y) => .5 * (x + y));
            for (int k = 0; k < Domain.Count; ++k)
            {
                Buffer2[k] = TimeSinuoids[k].Filter(src[k]);
                Buffer3[k] = TimeTransients[k].Filter(Buffer1[k]);
            }
            FrequencySinuoids.FilterSpan(Buffer2, Buffer4, (x, y) => .5 * (x + y));

            Array.Clear(KeyWeights, 0, KeyWeights.Length);
            var noiseFloor = Enumerable.Range(0, input.Length).Sum(k => input.Span[k]) / input.Length;

            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                var sinuoids = Math.Max(0, Buffer2[k] - Buffer4[k]);
                var transients = Math.Max(0, Buffer1[k] - Buffer3[k]);
                var noise = Math.Max(0, src[k] - noiseFloor);

                KeyWeights[k % KeyWeights.Length] += sinuoids * Domain.Resolution / Domain.Count;

                //y[0] = Rolloff(sinuoids);
                //y[1] = Rolloff(transients);
                //y[2] = Rolloff(noise);
                //y[3] = 0;

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
