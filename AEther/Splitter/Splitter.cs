using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{

    public class Splitter
    {

        readonly Domain Domain;

        readonly IFrequencyFilter<float> Frequency;
        readonly ITimeFilter<float>[] Time;

        readonly IFrequencyFilter<float> Frequency2;
        readonly ITimeFilter<float>[] Time2;

        readonly float FloorRoom;
        readonly MovingQuantile Ceiling;

        readonly float[] Buffer1;
        readonly float[] Buffer2;
        readonly float[] Buffer3;
        readonly float[] Buffer4;

        public Splitter(Domain domain, float timeResolution, float frequencyWindow, float timeWindow, float floorRoom, float headRoom)
        {

            Domain = domain;
            FloorRoom = floorRoom;

            int halfSizeFrequency = (int)(frequencyWindow * domain.Resolution);
            int halfSizeTime = (int)(timeWindow * timeResolution);

            Buffer1 = new float[Domain.Count];
            Buffer2 = new float[Domain.Count];
            Buffer3 = new float[Domain.Count];
            Buffer4 = new float[Domain.Count];

            //Frequency = new MovingExponentialAverage(2f / (1 + halfSizeFrequency));
            //Time = Enumerable.Range(0, domain.Count)
            //    .Select(k => new MovingExponentialAverage(2f / (1 + halfSizeTime)))
            //    .ToArray();

            //Frequency2 = new MovingExponentialAverage(2f / (1 + halfSizeFrequency));
            //Time2 = Enumerable.Range(0, domain.Count)
            //    .Select(k => new MovingExponentialAverage(2f / (1 + halfSizeTime)))
            //    .ToArray();

            Frequency = new MovingMedian(1 + halfSizeFrequency);
            Time = Enumerable.Range(0, domain.Count)
                .Select(k => new MovingMedian(1 + halfSizeTime))
                .ToArray();

            Frequency2 = new MovingMedian(1 + halfSizeFrequency);
            Time2 = Enumerable.Range(0, domain.Count)
                .Select(k => new MovingMedian(1 + halfSizeTime))
                .ToArray();

            Ceiling = new MovingQuantile(1 - headRoom, 2f / (1 + 7 * timeResolution));
        }

        public void Process(Memory<float> input, Memory<float> output)
        {

            var src = input.Span;
            var dst = output.Span;

            Frequency.Filter(src, Buffer1);
            for (int k = 0; k < Domain.Count; ++k)
            {
                Buffer2[k] = Time[k].Filter(src[k]);
                Buffer3[k] = Time2[k].Filter(Buffer1[k]);
            }
            Frequency2.Filter(Buffer2, Buffer4);

            var currentCeiling = float.NegativeInfinity;
            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                y[0] = Math.Max(0, Buffer2[k] - Buffer4[k]);
                y[1] = Math.Max(0, Buffer1[k] - Buffer3[k]);
                //y[2] = Math.Max(0, src[k] - Math.Max(y[0], y[1]))
                y[2] = Math.Max(0, src[k] - y[0] - y[1]);
                //y[3] = 0;

                //y[0] = Math.Max(buffer2[k], buffer4[k]);
                //y[1] = Math.Max(buffer1[k], buffer3[k]);
                //y[2] = 0;
                y[3] = 0;

                for(int i = 0; i < 4; ++i)
                {
                    currentCeiling = Math.Max(currentCeiling, y[i]);
                }

                //for (int i = 0; i < 4; ++i)
                //{
                //    y[i] = y[i].Clip(0f, 1f);
                //}
            }

            var ceiling = Ceiling.Filter(currentCeiling);

            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                for (int i = 0; i < 4; ++i)
                {
                    y[i] = ((1 + FloorRoom) * (y[i] / ceiling) - FloorRoom).Clamp(0, 1);
                }
            }

        }

    }
}
