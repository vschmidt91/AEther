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

        readonly int Width = 100;
        readonly int Count = 1 << 15;
        readonly int Seed = 123;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestMedianArray()
        {
            var median = new MovingMedian(Width);
            var b = new float[Width];
            var rng = new Random(Seed);
            for (var i = 0; i < Count; ++i)
            {
                median.Filter((float)rng.NextDouble());
                var expected = median.Buffer.OrderBy(v => v).ToArray();
                Assert.That(median.Sorted, Is.EqualTo(expected));
            }
        }

        [Test]
        public void TestMedianBST()
        {
            var median1 = new MovingMedian(Width);
            for (var i = 0; i < 1 + 2 * Width; ++i)
            {
                median1.Filter(0f);
            }
            var median2 = new MovingMedianBST<float>(Enumerable.Repeat(0f, 2 * Width + 1));
            var rng = new Random(Seed);
            for (var i = 0; i < Count; ++i)
            {
                var x = (float)rng.NextDouble();
                var m1 = median1.Filter(x);
                var m2 = median2.Filter(x);
                Assert.AreEqual(m1, m2);
            }
        }

        [Test]
        public void TestMedianHeap()
        {
            var median1 = new MovingMedian(Width);
            for (var i = 0; i < 1 + 2 * Width; ++i)
            {
                median1.Filter(0f);
            }
            var median2 = new MovingMedianHeap<float>(Enumerable.Repeat(0f, 2 * Width + 1));
            var rng = new Random(Seed);
            for (var i = 0; i < Count; ++i)
            {
                var x = (float)rng.NextDouble();
                var m1 = median1.Filter(x);
                var m2 = median2.Filter(x);
                Assert.AreEqual(m1, m2);
            }
        }

    }
}
