﻿using System;
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

        readonly MovingFilter<double> Frequency;
        readonly MovingFilter<double>[] Time;

        readonly MovingFilter<double> Frequency2;
        readonly MovingFilter<double>[] Time2;

        readonly double[] Buffer1;
        readonly double[] Buffer2;
        readonly double[] Buffer3;
        readonly double[] Buffer4;

        readonly double[] KeyWeights;

        public Splitter(Domain domain, double timeResolution, double frequencyWindow, double timeWindow)
        {

            Domain = domain;

            int halfSizeFrequency = (int)(frequencyWindow * domain.Resolution);
            int halfSizeTime = (int)(timeWindow * timeResolution);

            KeyWeights = new double[Domain.Resolution];
            Buffer1 = new double[Domain.Count];
            Buffer2 = new double[Domain.Count];
            Buffer3 = new double[Domain.Count];
            Buffer4 = new double[Domain.Count];

            Frequency = CreateFilter(1 + 2 * halfSizeFrequency);
            Time = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter(1 + 2 * halfSizeTime))
                .ToArray();

            Frequency2 = CreateFilter(1 + 2 * halfSizeFrequency);
            Time2 = Enumerable.Range(0, domain.Count)
                .Select(k => CreateFilter(1 + 2 * halfSizeTime))
                .ToArray();

        }

        static WindowedFilter<double> CreateFilter(int windowSize)
            => new MovingMedianArray<double>(windowSize, Comparer<double>.Default);

        public void Process(ReadOnlyMemory<double> input, Memory<double> output)
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
