using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public readonly float[] Sorted;
        public readonly float[] Buffer;

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

            var oldValue = Buffer[Position];
            Buffer[Position] = value;

            Position++;
            if (Position == Buffer.Length)
                Position = 0;

            var i = BinarySearch(Sorted, oldValue, oldValue < value);
            var j = BinarySearch(Sorted, value, value < oldValue);

            if (oldValue < value)
            {
                Array.Copy(Sorted, i + 1, Sorted, i, j - i);
            }
            else if (value < oldValue)
            {
                Array.Copy(Sorted, j, Sorted, j + 1, i - j);
            }
            Sorted[j] = value;

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

        public static int BinarySearch(float[] data, float value, bool left = true)
            => left
                ? BinarySearchLeft(data, value)
                : BinarySearchRight(data, value);

        public static int BinarySearchRight(float[] data, float value)
        {

            var l = 0;
            var r = data.Length;

            while(l < r)
            {
                var m = (l + r) / 2;
                if (value < data[m])
                    r = m;
                else
                    l = m + 1;
            }

            return r - 1;

        }

        public static int BinarySearchLeft(float[] data, float value)
        {

            var l = 0;
            var r = data.Length;

            while (l < r)
            {
                var m = (l + r) / 2;
                if (data[m] < value)
                    l = m + 1;
                else
                    r = m;
            }

            return l;

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
                Buffer[i] = ((i & 1) == 1)
                    ? float.NegativeInfinity
                    : float.PositiveInfinity;
                Sorted[i] = i < HalfWidth
                    ? float.NegativeInfinity
                    : float.PositiveInfinity;
            }

        }

    }
}
