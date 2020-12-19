using System;
using System.Collections.Generic;
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
            NuttalWindow,
            BlackmanWindow,
            HannWindow,
            RectWindow,
        };

        readonly IDFTFilter[] Filters;
        readonly float[] AWeighting;
        readonly bool UseParallelization;

        public DFTProcessor(Domain domain, float sampleRate, bool useSIMD, bool useParallelization)
        {

            var cosines = useSIMD
                ? WindowCandidates.First(w => 2 * w.Length - 1 <= Vector<float>.Count)
                : HannWindow;

            cosines = RectWindow;

            Filters = new IDFTFilter[domain.Count];
            UseParallelization = useParallelization;
            for(int k = 0; k < domain.Count; ++k)
            {
                if(cosines.Length == 1)
                {
                    Filters[k] = new DFTFilter(domain[k], domain.Resolution, sampleRate);
                }
                else if(useSIMD)
                {
                    Filters[k] = new WindowedDFTFilterSIMD(domain[k], domain.Resolution, sampleRate, CreateWindow(cosines));
                }
                else
                {
                    Filters[k] = new WindowedDFTFilter(domain[k], domain.Resolution, sampleRate, CreateWindow(cosines));
                }
            }

            var a1k = GetAWeighting(1000);
            AWeighting = domain.Select(f => (float)(GetAWeighting(f) / a1k)).ToArray();
            //AWeighting = domain.Select(f => f / 1000f).ToArray();

        }

        static double GetAWeighting(double f)
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

        static float[] CreateWindow(float[] cosines)
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

        public void Output(Span<float> dst)
        {
            for (int k = 0; k < Filters.Length; ++k)
            {
                var filter = Filters[k];
                var bin = filter.GetOutput().Magnitude;

                if (bin == 0)
                {
                    dst[k] = 0f;
                }
                else
                {
                    var scaled = 2 * bin / filter.Length;
                    dst[k] = (float)Math.Max(-10, Math.Log(scaled));
                }
            }
        }

        public void Process(ReadOnlyMemory<float> samples)
        {

            if (UseParallelization)
            {
                Parallel.ForEach(Filters, filter =>
                {
                    for (int i = 0; i < samples.Length; ++i)
                    {
                        filter.Process(samples.Span[i]);
                    }
                });
            }
            else
            {
                for (int k = 0; k < Filters.Length; ++k)
                {
                    for (int i = 0; i < samples.Length; ++i)
                    {
                        Filters[k].Process(samples.Span[i]);
                    }
                }
            }

        }

    }
}
