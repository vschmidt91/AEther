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
        readonly double Frequency;
        readonly double DFTFrequency;
        readonly double Q;
        readonly int M;
        readonly RingBuffer<double> Buffer;

        public DFTFilter(double frequency, double frequencyResolution, double sampleRate)
        {

            Frequency = frequency;
            DFTFrequency = Frequency / sampleRate;
            Q = 1.0 / (Math.Pow(2, 1.0 / frequencyResolution) - 1);
            M = (int)Math.Round(Q);
            State = Complex.Zero;
            var length = (int)Math.Round(M / DFTFrequency);
            Buffer = new(length);
            Coefficient = Complex.Exp(2 * Math.PI * Complex.ImaginaryOne * M / Length);

        }

        public Complex GetOutput()
        {
            return State / Buffer.Size;
        }

        public void Process(double newSample)
        {
            var oldSample = Buffer.Add(newSample);
            var input = newSample - oldSample;
            State = Coefficient * State + input;
        }

        public void Process(ReadOnlyMemory<double> samples)
        {
            while(0 < samples.Length)
            {

                var dst = Buffer.GetMemory(samples.Length);

                for (var i = 0; i < dst.Length; ++i)
                {
                    var input = samples.Span[i] - dst.Span[i];
                    State = Coefficient * State + input;
                }

                samples[0..dst.Length].CopyTo(dst);
                samples = samples[dst.Length..];

            }
        }

    }
}
