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

        static readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;

        readonly Domain Domain;

        readonly IFrequencyFilter<float> Frequency;
        readonly ITimeFilter<float>[] Time;

        readonly IFrequencyFilter<float> Frequency2;
        readonly ITimeFilter<float>[] Time2;

        readonly float FloorRoom;
        readonly MovingQuantile Ceiling;

        public Splitter(Domain domain, float timeResolution, float frequencyWindow, float timeWindow, float floorRoom, float headRoom)
        {

            Domain = domain;
            FloorRoom = floorRoom;

            int halfSizeFrequency = (int)(frequencyWindow * domain.Resolution);
            int halfSizeTime = (int)(timeWindow * timeResolution);

            Frequency = new MovingExponentialAverage(2f / (1 + halfSizeFrequency));
            Time = Enumerable.Range(0, domain.Count)
                .Select(k => new MovingExponentialAverage(2f / (1 + halfSizeTime)))
                .ToArray();

            Frequency2 = new MovingExponentialAverage(2f / (1 + halfSizeFrequency));
            Time2 = Enumerable.Range(0, domain.Count)
                .Select(k => new MovingExponentialAverage(2f / (1 + halfSizeTime)))
                .ToArray();

            Ceiling = new MovingQuantile(1 - headRoom, 2f / (1 + 7 * timeResolution));
        }

        public void Process(Span<float> input, Span<float> output)
        {

            var src = input;
            var dst = output;

            var buffer1 = Pool.Rent(Domain.Count);
            var buffer2 = Pool.Rent(Domain.Count);
            var buffer3 = Pool.Rent(Domain.Count);
            var buffer4 = Pool.Rent(Domain.Count);

            Frequency.Filter(src, buffer1);
            for (int k = 0; k < Domain.Count; ++k)
            {
                buffer2[k] = Time[k].Filter(src[k]);
                buffer3[k] = Time2[k].Filter(buffer1[k]);
            }
            Frequency2.Filter(buffer2, buffer4);

            var currentCeiling = float.NegativeInfinity;
            for (int k = 0; k < Domain.Count; ++k)
            {

                var y = dst.Slice(4 * k, 4);

                y[0] = Math.Max(0, buffer2[k] - buffer4[k]);
                y[1] = Math.Max(0, buffer1[k] - buffer3[k]);
                y[2] = Math.Max(0, src[k] - Math.Min(buffer3[k], buffer4[k]));
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

            Pool.Return(buffer1);
            Pool.Return(buffer2);
            Pool.Return(buffer3);
            Pool.Return(buffer4);

        }

    }
}
