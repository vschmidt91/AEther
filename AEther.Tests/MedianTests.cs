using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace AEther.Tests
{
    class MedianTests
    {

        readonly int Width = 1 << 8;
        readonly int Count = 1 << 16;
        readonly int Seed = 123;
        readonly Comparison<float> Comparison = Comparer<float>.Default.Compare;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestMedianArray()
            => TestMedian(new MovingMedianRef<float>(Width, Comparison), new MovingMedianArray<float>(Width, Comparison), Count, Seed);

        //[Test]
        //public void TestMedianBST()
        //    => TestMedian(new MovingMedianRef<float>(Width, Comparer), new MovingMedianBST<float>(Width, Comparer), Count, Seed);

        [Test]
        public void TestMedianHeap()
            => TestMedian(new MovingMedianRef<float>(Width, Comparison), new MovingMedianHeap<float>(Width, Comparison), Count, Seed);

        static void TestMedian(MovingFilter<float> a, MovingFilter<float> b, int length, int seed = 0)
        {
            var rng = new Random(seed);
            for (var i = 0; i < length; ++i)
            {
                var x = (float)rng.NextDouble();
                var y1 = a.Filter(x);
                var y2 = b.Filter(x);
                Assert.AreEqual(y1, y2);
            }
        }

    }
}
