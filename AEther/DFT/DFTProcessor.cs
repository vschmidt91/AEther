using System;
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

        public static readonly float[] RectWindow = new[] { 1f };
        public static readonly float[] HannWindow = new[] { 0.5f, 0.5f };
        public static readonly float[] HammingWindow = new[] { .54f, .46f };
        public static readonly float[] HammingExactWindow = new[] { 25 / 46f, 21 / 46f };
        public static readonly float[] BlackmanWindow = new[] { .42f, .5f, .08f };
        public static readonly float[] BlackmanExactWindow = new[] { 3969 / 9304f, 4620 / 9304f, 715 / 9304f };
        public static readonly float[] NuttalWindow = new[] { .355768f, .487396f, .144232f, .012604f };

        public static readonly float[][] WindowCandidates = new[]
        {
            RectWindow,
            HannWindow,
            BlackmanWindow,
            NuttalWindow,
        };

        readonly IDFTFilter[] Filters;
        readonly ParallelOptions ParallelOptions;

        public DFTProcessor(Domain domain, float sampleRate, bool useSIMD = true, int maxParallelism = -1)
        {
            
            Console.WriteLine(Vector<float>.Count);

            var cosines = useSIMD
                ? WindowCandidates.Last(w => 2 * w.Length - 1 <= Vector<float>.Count)
                : HannWindow;

            //cosines = RectWindow;

            var window = CreateWindow(cosines);

            ParallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallelism,
            };

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

        public static float[] CreateWindow(float[] cosines)
        {
            var window = new float[2 * cosines.Length - 1];
            window[cosines.Length - 1] = cosines[0];
            for (int j = 1; j < cosines.Length; ++j)
            {
                var coeff = 0.5f * (float)Math.Pow(-1, j) * cosines[j];
                window[cosines.Length - 1 - j] = window[cosines.Length - 1 + j] = coeff;
            }
            return window;
        }

        public void Output(Memory<float> dst)
        {
            for (int k = 0; k < Filters.Length; ++k)
            {
                var filter = Filters[k];
                var bin = 2 * filter.GetOutput().Magnitude;
                dst.Span[k] = (float)Math.Log10(Math.Max(1e-10, bin));
            }
        }

        public void Process(ReadOnlyMemory<float> samples)
        {

            void ProcessFilter(IDFTFilter filter)
            {
                filter.Process(samples);
            }

            if (ParallelOptions.MaxDegreeOfParallelism == 1)
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
