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
    public class WindowedDFTFilterSIMD : IDFTFilter
    {

        public int Length => Buffer.Size;

        ComplexVector<float> States;
        readonly ComplexVector<float> Coefficients;
        readonly float Frequency;
        readonly float DFTFrequency;
        readonly float Q;
        readonly int M;
        readonly Vector<float> Window;
        readonly int WindowSize;
        readonly RingBuffer<float> Buffer;

        public WindowedDFTFilterSIMD(float frequency, float frequencyResolution, float sampleRate, float[] window)
        {

            WindowSize = window.Length;
            var windowPadded = window.Concat(Enumerable.Repeat(0f, Vector<float>.Count - window.Length)).ToArray();
            Window = new Vector<float>(windowPadded);
            Frequency = frequency;
            DFTFrequency = Frequency / sampleRate;
            Q = 1f / (float)(Math.Pow(2, 1f / frequencyResolution) - 1);
            M = (int)Math.Round(Q);
            States = ComplexVector<float>.Zero;
            var length = (int)Math.Round(M / DFTFrequency);
            Buffer = new(length);
            Coefficients = ComplexVector<float>.Zero;
            var coeff = new Complex[ComplexVector<float>.Count];
            for (int j = 0; j < window.Length; ++j)
            {
                var bin = M - (window.Length / 2) + 1 + j;
                coeff[j] = Complex.Exp(2 * Math.PI * Complex.ImaginaryOne * bin / Length);
            }
            var coeffRe = new Vector<float>(coeff.Select(c => (float)c.Real).ToArray());
            var coeffIm = new Vector<float>(coeff.Select(c => (float)c.Imaginary).ToArray());
            Coefficients = new ComplexVector<float>(coeffRe, coeffIm);

        }

        public Complex GetOutput()
        {
            var windowed = Window * States;
            Complex result = Complex.Zero;
            for (int j = 0; j < WindowSize; ++j)
            {
                var (re, im) = windowed[j];
                result += new Complex(re, im);
            }
            return result / Buffer.Size;
        }

        public void Process(float newSample)
        {
            var oldSample = Buffer.Add(newSample);
            var input = new Vector<float>(newSample - oldSample);
            States = Coefficients * States + input;
        }

        public void Process(ReadOnlyMemory<float> samples)
        {
            while (0 < samples.Length)
            {
                var dst = Buffer.GetMemory(samples.Length);
                for (var i = 0; i < dst.Length; ++i)
                {
                    var input = new Vector<float>(samples.Span[i] - dst.Span[i]);
                    States = Coefficients * States + input;
                }
                samples[0..dst.Length].CopyTo(dst);
                samples = samples[dst.Length..];
            }
        }
    }
}
