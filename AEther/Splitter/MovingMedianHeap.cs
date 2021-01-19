using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AEther
{ 

    public class MovingMedianHeap
    {

        readonly MaxHeap<float> Below;
        readonly MinHeap<float> Above;

        readonly float[] Buffer;
        int Position = 0;

        public float Median { get; protected set; } = 0;

        public MovingMedianHeap(int size)
        {
            var items = Enumerable.Repeat(0f, size);
            Buffer = items.ToArray();
            Below = new(items.Take(size / 2));
            Above = new(items.Skip(size / 2));
            SetMedian();
        }

        public void Add(float value)
        {

            var oldValue = Buffer[Position];
            Buffer[Position] = value;

            Position += 1;
            if (Position == Buffer.Length)
            {
                Position = 0;
            }

            if (oldValue == Median)
            {
                if (Below.Count < Above.Count)
                {
                    Above.Remove(oldValue);
                }
                else
                {
                    Below.Remove(oldValue);
                }
            }
            else if (oldValue < Median)
            {
                Below.Remove(oldValue);
            }
            else
            {
                Above.Remove(oldValue);
            }

            if (value < Median)
            {
                Below.Add(value);
            }
            else
            {
                Above.Add(value);
            }

            // if the difference is more than one
            if (Math.Abs(Below.Count - Above.Count) > 1)
            {
                if (Below.Count < Above.Count)
                {
                    Below.Add(Above.ExtractDominating());
                }
                else
                {
                    Above.Add(Below.ExtractDominating());
                }
            }

            SetMedian();

        }

        void SetMedian()
        {
            // calculate the median
            if (Below.Count == Above.Count)
            {
                Median = .5f * (Below.First() + Above.First());
            }
            else if (Below.Count < Above.Count)
            {
                Median = Above.First();
            }
            else
            {
                Median = Below.First();
            }
        }

    }

}
