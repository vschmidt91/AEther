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
    public class DFTFilter : IDFTFilter
    {

        public int Length => Buffer.Size;

        Complex State;

        readonly Complex Coefficient;
        readonly float Frequency;
        readonly float DFTFrequency;
        readonly float Q;
        readonly int M;
        readonly RingBuffer<float> Buffer;

        public DFTFilter(float frequency, float frequencyResolution, float sampleRate)
        {

            Frequency = frequency;
            DFTFrequency = Frequency / sampleRate;
            Q = 1f / (float)(Math.Pow(2, 1f / frequencyResolution) - 1);
            M = (int)Math.Round(Q);
            State = Complex.Zero;
            var length = (int)Math.Round(M / DFTFrequency);
            Buffer = RingBuffer<float>.Create(length);
            Coefficient = Complex.Exp(2 * Math.PI * Complex.ImaginaryOne * M / Length);

        }

        public Complex GetOutput()
        {
            return State;
        }

        public void Process(float newSample)
        {
            var oldSample = Buffer.Add(newSample);
            var input = newSample - oldSample;
            State = Coefficient * State + input;
        }

        public void Process(ReadOnlyMemory<float> samples)
        {
            while(0 < samples.Length)
            {

                var (src, dst) = Buffer.Add(samples);

                for (var i = 0; i < src.Length; ++i)
                {
                    var input = src.Span[i] - dst.Span[i];
                    State = Coefficient * State + input;
                }

                //var newState = Complex.Zero;
                //for (var i = 0; i < src.Length; ++i)
                //{
                //    var coeff = CoefficientPow[src.Length - 1 - i];
                //    var input = src.Span[i] - dst.Span[i];
                //    newState = newState + coeff * input;
                //}
                //State = CoefficientPow[src.Length] * State + newState;

                //var add = Enumerable.Range(0, src.Length);
                //var addRe = add.Sum(i => (src.Span[i] - dst.Span[i]) * CoefficientPow[src.Length - 1 - i].Real);
                //var addIm = add.Sum(i => (src.Span[i] - dst.Span[i]) * CoefficientPow[src.Length - 1 - i].Imaginary);
                //State = CoefficientPow[src.Length] * State + new Complex(addRe, addIm);

                Buffer.Advance(src);
                samples = samples[src.Length..];

            }
        }

    }
}
