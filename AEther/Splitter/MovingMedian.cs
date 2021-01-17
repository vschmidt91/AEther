using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class MovingMedian : ITimeFilter<float>, IFrequencyFilter<float>
    {

        public readonly int HalfWidth;
        public int Width => 2 * HalfWidth + 1;

        readonly float[] Sorted;
        readonly float[] Buffer;

        int Position;
        bool NoneFlag;

        public MovingMedian(int halfWidth)
        {

            HalfWidth = halfWidth;

            Sorted = new float[Width];
            Buffer = new float[Width];

            Clear();

        }

        public void Clear()
        {
            ClearOpt();
        }

        public void Filter(ReadOnlySpan<float> src, Memory<float> dst)
        {

            Clear();

            for (int k = 0; k < HalfWidth; ++k)
            {
                Filter(src[k]);
            }

            for (int k = HalfWidth; k < src.Length; ++k)
            {
                dst.Span[k - HalfWidth] = Filter(src[k]);
            }

            for (int k = src.Length - HalfWidth; k < src.Length; ++k)
            {
                dst.Span[k] = FilterNone();
            }

        }

        public float Filter(float value)
        {

            float oldValue = Buffer[Position];
            Buffer[Position] = value;

            if (++Position == Buffer.Length)
                Position = 0;

            if (value != oldValue)
            {

                int i = BinarySearch(Sorted, oldValue);

                if (value < oldValue)
                {
                    while (i > 0 && value < Sorted[i - 1])
                        Sorted[i] = Sorted[--i];

                }
                else
                {
                    while (i < Buffer.Length - 1 && Sorted[i + 1] < value)
                        Sorted[i] = Sorted[++i];
                }

                Sorted[i] = value;

            }

            return Sorted[HalfWidth];

        }

        float FilterNone()
        {
            NoneFlag = !NoneFlag;
            if (NoneFlag)
                return Filter(float.NegativeInfinity);
            else
                return Filter(float.PositiveInfinity);
        }

        static int BinarySearch(float[] data, float value)
        {

            int low = 0;
            int high = data.Length - 1;
            int mid;
            float midValue;

            while (low + 1 < high)
            {
                mid = (low + high) / 2;
                midValue = data[mid];
                if (value < midValue)
                    high = mid - 1;
                else if (midValue < value)
                    low = mid + 1;
                else
                    return mid;
            }

            if (low + 1 == high && data[low] < value)
                return low + 1;
            else
                return low;

        }

        public void ClearValue(float value)
        {
            Array.Fill(Buffer, value);
            Array.Fill(Sorted, value);
        }

        public void ClearRef()
        {
            Position = 0;
            Array.Clear(Buffer, 0, Buffer.Length);
            Array.Clear(Sorted, 0, Sorted.Length);
            for (int i = 0; i < Width; ++i)
                FilterNone();
        }

        public void ClearOpt()
        {

            Position = 0;
            for (int i = 0; i < Width; ++i)
            {
                Buffer[i] = ((i & 1) == 0)
                    ? float.PositiveInfinity
                    : float.NegativeInfinity;
                Sorted[i] = i < HalfWidth
                    ? float.NegativeInfinity
                    : float.PositiveInfinity;
            }

        }

        public void Check()
        {
            var b = new float[Width];
            Array.Copy(Buffer, 0, b, 0, Width);
            Array.Sort(b);
            Debug.Assert(Sorted.SequenceEqual(b));
        }

    }
}
