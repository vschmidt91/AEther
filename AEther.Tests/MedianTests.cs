﻿
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AEther.Tests
{
    class MedianTests
    {

        readonly int Width = 1 << 8;
        readonly int Count = 1 << 16;
        readonly int Seed = 123;
        readonly double State = 0.0;
        readonly Comparer<double> Comparer = Comparer<double>.Default;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestMedianArray()
            => TestMedian(new MovingMedianRef<double>(State, Width, Comparer), new MovingMedianArray<double>(State, Width, Comparer), Count, Seed);

        [Test]
        public void TestMedianBST()
            => TestMedian(new MovingMedianRef<double>(State, Width, Comparer), new MovingMedianBST<double>(State, Width, Comparer), Count, Seed);

        [Test]
        public void TestMedianHeap()
            => TestMedian(new MovingMedianRef<double>(State, Width, Comparer), new MovingMedianHeap<double>(State, Width, Comparer), Count, Seed);

        static void TestMedian(MovingFilter<double> a, MovingFilter<double> b, int length, int seed = 0)
        {
            var rng = new Random(seed);
            for (var i = 0; i < length; ++i)
            {
                var x = rng.NextDouble();
                a.Filter(x);
                b.Filter(x);
                Assert.AreEqual(a.State, b.State);
            }
        }

    }
}
