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
            var width = 1;
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

    }
}
