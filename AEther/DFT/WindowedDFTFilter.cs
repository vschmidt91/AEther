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
        readonly double[] Window;
        readonly RingBuffer<double> Buffer;

        public WindowedDFTFilter(float frequency, float frequencyResolution, float sampleRate, double[] window)
        {

            Window = window;
            Frequency = frequency;
            DFTFrequency = Frequency / sampleRate;
            Q = 1f / (float)(Math.Pow(2, 1f / frequencyResolution) - 1);
            M = (int)Math.Round(Q);
            States = new Complex[Window.Length];
            var length = (int)Math.Round(M / DFTFrequency);
            Buffer = new(length);
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
            return result / Buffer.Size;
        }

        public void Process(double newSample)
        {
            var oldSample = Buffer.Add(newSample);
            var input = newSample - oldSample;
            for (int j = 0; j < Window.Length; ++j)
            {
                States[j] = Coefficients[j] * States[j] + input;
            }
        }

        public void Process(ReadOnlyMemory<double> samples)
        {
            while (0 < samples.Length)
            {
                var dst = Buffer.GetMemory(samples.Length);
                for (var i = 0; i < dst.Length; ++i)
                {
                    var input = samples.Span[i] - dst.Span[i];
                    for (int j = 0; j < Window.Length; ++j)
                    {
                        States[j] = Coefficients[j] * States[j] + input;
                    }
                }
                samples[0..dst.Length].CopyTo(dst);
                samples = samples[dst.Length..];
            }
        }
    }
}