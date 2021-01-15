using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther
{
    public class WindowedDFTFilter : IDFTFilter
    {

        public int Length => Buffer.Size;

        readonly Complex[] States;
        readonly Complex[] Coefficients;
        readonly float Frequency;
        readonly float DFTFrequency;
        readonly float Q;
        readonly int M;
        readonly float[] Window;
        readonly RingBuffer<float> Buffer;

        public WindowedDFTFilter(float frequency, float frequencyResolution, float sampleRate, float[] window)
        {

            Window = window;
            Frequency = frequency;
            DFTFrequency = Frequency / sampleRate;
            Q = 1f / (float)(Math.Pow(2, 1f / frequencyResolution) - 1);
            M = (int)Math.Round(Q);
            States = new Complex[Window.Length];
            var length = (int)Math.Round(M / DFTFrequency);
            Buffer = RingBuffer<float>.Create(length);
            Coefficients = new Complex[Window.Length];
            for (int j = 0; j < Window.Length; ++j)
            {
                var bin = M - (Window.Length / 2) + 1 + j;
                Coefficients[j] = Complex.Exp(2 * Complex.ImaginaryOne * Math.PI * bin / Length);
            }

        }

        public Complex GetOutput()
        {
            Complex result = Complex.Zero;
            for (int j = 0; j < Window.Length; ++j)
            {
                result += Window[j] * States[j];
            }
            return result;
        }

        public void Process(float newSample)
        {
            var oldSample = Buffer.Add(newSample);
            var input = newSample - oldSample;
            for (int j = 0; j < Window.Length; ++j)
            {
                States[j] = Coefficients[j] * States[j] + input;
            }
        }

        public void Process(ReadOnlyMemory<float> samples)
        {
            while (0 < samples.Length)
            {
                var (src, dst) = Buffer.Add(samples);
                for (var i = 0; i < src.Length; ++i)
                {
                    var input = src.Span[i] - dst.Span[i];
                    for (int j = 0; j < Window.Length; ++j)
                    {
                        States[j] = Coefficients[j] * States[j] + input;
                    }
                }
                Buffer.Advance(src);
                samples = samples[src.Length..];
            }
        }
    }
}