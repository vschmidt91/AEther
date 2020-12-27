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
            Coefficient = Complex.Exp(2 * Complex.ImaginaryOne * Math.PI * M / Length);

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
            foreach(var (src, dst) in Buffer.Add(samples))
            {
                for(var i = 0; i < src.Length; ++i)
                {
                    var input = src.Span[i] - dst.Span[i];
                    State = Coefficient * State + input;
                }
            }
        }

    }
}
