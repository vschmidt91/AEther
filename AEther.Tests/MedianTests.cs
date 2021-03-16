using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace AEther.Tests
{
    class MedianTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestMedian()
        {
            var width = 10;
            var median = new MovingMedian(width);
            var b = new float[width];
            var rng = new Random(0);
            for (var i = 0; i < 1 << 15; ++i)
            {
                median.Filter((float)rng.NextDouble());
                var expected = median.Buffer.OrderBy(v => v).ToArray();
                Assert.That(median.Sorted, Is.EqualTo(expected));
            }
        }

        [Test]
        public void TestMedian2()
        {
            var width = 10;
            var median1 = new MovingMedian(width);
            for (var i = 0; i < 1 + 2 * width; ++i)
            {
                median1.Filter(0f);
            }
            var median2 = new MovingMedianBST<float>(Enumerable.Repeat(0f, 2 * width + 1));
            var rng = new Random(0);
            for (var i = 0; i < 1 << 20; ++i)
            {
                var x = (float)rng.NextDouble();
                var m1 = median1.Filter(x);
                var m2 = median2.Filter(x);
                Assert.AreEqual(m1, m2);
            }
        }

    }
}
