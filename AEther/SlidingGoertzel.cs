using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AEther
{
    public class SlidingGoertzel : IDFT
    {

        internal struct StateElement
        {
            internal float Q0;
            internal float Q1;
            internal float Q2;
        }

        internal struct ConstElement
        {
            internal float Coeff;
            internal float Damping;
        }

        int ChannelCount => States.GetLength(0);

        readonly StateElement[,] States;
        readonly ConstElement[] Const;

        readonly Domain Domain;
        readonly Domain DFTDomain;
        readonly double Q;
        readonly int M;
        readonly int BatchSize;

        int SampleCounter;

        public SlidingGoertzel(Domain domain, int batchSize, float sampleRate, float damping, int channelCount=2)
        {

            BatchSize = batchSize;
            SampleCounter = BatchSize;

            Domain = domain;
            DFTDomain = new Domain(domain.MinValue / sampleRate, domain.Resolution, domain.Count);

            Q = 1.0 / (Math.Pow(2, 1.0 / domain.Resolution) - 1);
            M = (int)Math.Round(Q);

            States = new StateElement[channelCount, DFTDomain.Count];

            Const = Enumerable.Range(0, DFTDomain.Count)
                .Select(k =>
                {
                    var length = (int)Math.Round(Q / DFTDomain[k]);
                    var omega = 2 * Math.PI * M / length;
                    return new ConstElement
                    {
                        Coeff = 2 * (float)Math.Cos(omega),
                        //Damping = (float)Math.Pow(damping, 1.0 / length),
                        Damping = (float)Math.Pow(damping, 1000.0 / sampleRate),
                    };
                })
                .ToArray();

        }

        public void Clear()
        {
            Array.Clear(States, 0, States.Length);
            SampleCounter = BatchSize;
        }

        private static void DFTIteration(ref StateElement s, in ConstElement c, in float x)
        {
            s.Q2 = c.Damping * s.Q1;
            s.Q1 = c.Damping * s.Q0;
            s.Q0 = c.Coeff * s.Q1 - s.Q2 + x;
        }

        private static float DFTOutput(in StateElement s, in ConstElement c)
            => s.Q0 * s.Q0 + s.Q1 * s.Q1 - s.Q0 * s.Q1 * c.Coeff;

        public IEnumerable<DFTEvent> Filter(SampleEvent evt)
        {

            for (int i = 0; i < evt.Length; ++i)
            {

                for (int k = 0; k < Domain.Count; ++k)
                {
                    for (int c = 0; c < ChannelCount; ++c)
                    {

                        var x = evt[c].Span[i];
                        DFTIteration(ref States[c, k], Const[k], x);
                    }
                }

                SampleCounter--;
                if (SampleCounter <= 0)
                {
                    var result = DFTEvent.Rent(ChannelCount, Domain.Count);
                    for (int k = 0; k < Domain.Count; ++k)
                    {
                        for (int c = 0; c < ChannelCount; ++c)
                        {
                            var x = DFTOutput(States[c, k], Const[k]);
                            x = Math.Max(0, x);
                            x = DFTDomain[k] * (float)Math.Sqrt(x);
                            result.Channels[c][k] = x;
                        }
                    }
                    yield return new DFTEvent(result, Domain.Count, evt.Time);
                    SampleCounter = BatchSize;
                }

            }

            yield break;

        }

    }
}
