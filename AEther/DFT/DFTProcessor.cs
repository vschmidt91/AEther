﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class DFTProcessor
    {

        public static readonly double[] RectWindow = new[] { 1.0 };
        public static readonly double[] HannWindow = new[] { 0.5, 0.5 };
        public static readonly double[] HammingWindow = new[] { .54, .46 };
        public static readonly double[] HammingExactWindow = new[] { 25 / 46.0, 21 / 46.0 };
        public static readonly double[] BlackmanWindow = new[] { .42, .5, .08 };
        public static readonly double[] BlackmanExactWindow = new[] { 3969 / 9304.0, 4620 / 9304.0, 715 / 9304.0 };
        public static readonly double[] NuttalWindow = new[] { .355768, .487396, .144232, .012604 };

        public static readonly double[][] WindowCandidates = new[]
        {
            RectWindow,
            HannWindow,
            BlackmanWindow,
            NuttalWindow,
        };

        readonly IDFTFilter[] Filters;
        readonly ParallelOptions? ParallelOptions;

        public DFTProcessor(Domain domain, double sampleRate, bool useSIMD, int maxParallelism)
        {
            
            //Console.WriteLine(Vector<float>.Count);

            var cosines = useSIMD
                ? WindowCandidates.Last(w => 2 * w.Length - 1 <= Vector<double>.Count)
                : HannWindow;

            //cosines = RectWindow;

            var window = CreateWindow(cosines);

            if (0 < maxParallelism)
            {
                ParallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxParallelism,
                };
            }

            if (cosines.Length == 1)
            {
                Filters = domain
                    .Select(f => new DFTFilter(f, domain.Resolution, sampleRate))
                    .ToArray();
            }
            else if (useSIMD)
            {
                Filters = domain
                    .Select(f => new WindowedDFTFilterSIMD(f, domain.Resolution, sampleRate, window))
                    .ToArray();
            }
            else
            {
                Filters = domain
                    .Select(f => new WindowedDFTFilter(f, domain.Resolution, sampleRate, window))
                    .ToArray();
            }


        }

        public static double GetAWeighting(double f)
        {
            var f2 = f * f;
            var n = Math.Pow(12194.0, 2) * f2 * f2;
            var d1 = f2 + Math.Pow(20.6, 2);
            var d2 = f2 + Math.Pow(107.7, 2);
            var d3 = f2 + Math.Pow(737.9, 2);
            var d4 = f2 + Math.Pow(12194.0, 2);
            var r = n / (d1 * Math.Sqrt(d2 * d3) * d4);
            return r;
        }

        public static double[] CreateWindow(double[] cosines)
        {
            var window = new double[2 * cosines.Length - 1];
            window[cosines.Length - 1] = cosines[0];
            for (int j = 1; j < cosines.Length; ++j)
            {
                var coeff = 0.5 * Math.Pow(-1, j) * cosines[j];
                window[cosines.Length - 1 - j] = window[cosines.Length - 1 + j] = coeff;
            }
            return window;
        }

        public void Output(Memory<double> dst)
        {
            for (int k = 0; k < Filters.Length; ++k)
            {
                var filter = Filters[k];
                var bin = 2 * filter.GetOutput().Magnitude;
                dst.Span[k] = Math.Log10(Math.Max(1e-10, bin));
            }
        }

        public void Process(ReadOnlyMemory<double> samples)
        {

            void ProcessFilter(IDFTFilter filter)
            {
                filter.Process(samples);
            }

            if (ParallelOptions == null)
            {
                foreach (var filter in Filters)
                {
                    ProcessFilter(filter);
                }
            }
            else
            {
                Parallel.ForEach(Filters, ParallelOptions, ProcessFilter);
            }


        }

    }
}
