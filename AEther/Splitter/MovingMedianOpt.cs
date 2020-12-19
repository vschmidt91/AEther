using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public class MovingMedianOpt
    {

        readonly Heap<float> MinHeap;
        readonly Heap<float> MaxHeap;

        float Median;

        public MovingMedianOpt(int size)
        {
            MinHeap = new Heap<float>(size, Comparer<float>.Create((x, y) => x.CompareTo(y)));
            MaxHeap = new Heap<float>(size, Comparer<float>.Create((x, y) => y.CompareTo(x)));
            Median = 0f;
        }

        public float Filter(float x)
        {
            float y;
            if(x < Median)
            {
                y = MaxHeap.Rotate(x);
                if(Median < y)
                {
                    Median = MinHeap.Rotate(y);
                }
                else
                {
                    Median = y;
                }
            }
            else
            {
                y = MinHeap.Rotate(x);
                if (y < Median)
                {
                    Median = MinHeap.Rotate(y);
                }
                else
                {
                    Median = y;
                }
            }
            return Median;
        }

    }
}
