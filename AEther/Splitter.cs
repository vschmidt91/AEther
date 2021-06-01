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

        readonly MovingFilter<float> Frequency;
        readonly MovingFilter<float>[] Time;

        readonly MovingFilter<float> Frequency2;
        readonly MovingFilter<float>[] Time2;

        readonly float[] Buffer1;
        readonly float[] Buffer2;
        readonly float[] Buffer3;
        readonly float[] Buffer4;

        readonly float[] KeyWeights;

        public Splitter(Domain domain, float timeResolution, float frequencyWindow, float timeWindow)
        {

            Domain = domain;

            int halfSizeFrequency = (int)(frequencyWindow * domain.Resolution);
            int halfSizeTime = (int)(timeWindow * timeResolution);

            KeyWeights = new float[Domain.Resolution];
            Buffer1 = new float[Domain.Count];
            Buffer2 = new float[Domain.Count];
            Buffer3 = new float[Domain.Count];
            Buffer4 = new float[Domain.Count];

            Frequency = CreateFilter(1 + 2 * halfSizeFrequency);
            Time = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter(1 + 2 * halfSizeTime))
                .ToArray();

            Frequency2 = CreateFilter(1 + 2 * halfSizeFrequency);
            Time2 = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter(1 + 2 * halfSizeTime))
                .ToArray();

        }

        static WindowedFilter<float> CreateFilter(int windowSize)
            => new MovingMedianArray<float>(windowSize, Comparer<float>.Default);

        public void Process(ReadOnlyMemory<float> input, Memory<float> output)
        {

            var src = input.Span;
            var dst = output.Span;

            Frequency.FilterSpan(src, Buffer1, (x, y) => .5f * (x + y));
            for (int k = 0; k < Domain.Count; ++k)
            {
                Buffer2[k] = Time[k].Filter(src[k]);
                Buffer3[k] = Time2[k].Filter(Buffer1[k]);
            }
            Frequency2.FilterSpan(Buffer2, Buffer4, (x, y) => .5f * (x + y));

            Array.Clear(KeyWeights, 0, KeyWeights.Length);

            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                var sinuoids = Math.Max(0, Buffer2[k] - Buffer4[k]);
                var transients = Math.Max(0, Buffer1[k] - Buffer3[k]);

                KeyWeights[k % KeyWeights.Length] += sinuoids * Domain.Resolution / Domain.Count;

                y[0] = sinuoids;
                y[1] = transients;
                //y[2] = 1 + .1f * src[k];
                y[2] = 0;
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
