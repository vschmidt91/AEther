using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AEther
{
    public readonly struct ComplexVector<T>
        where T : struct
    {

        public static readonly ComplexVector<T> Zero = new ComplexVector<T>(Vector<T>.Zero, Vector<T>.Zero);
        public static readonly ComplexVector<T> One = new ComplexVector<T>(Vector<T>.One, Vector<T>.Zero);
        public static readonly ComplexVector<T> ImaginaryOne = new ComplexVector<T>(Vector<T>.Zero, Vector<T>.One);

        public readonly Vector<T> Real;
        public readonly Vector<T> Imaginary;

        public static int Count = Vector<T>.Count;

        public (T, T) this[int i] => (Real[i], Imaginary[i]);

        public T LengthSquared => Enumerable.Range(0, Count).Aggregate<int, T>(default, LengthSquaredAggregate);

        T LengthSquaredAggregate(T sum, int i)
        {
            var re = Real[i];
            var im = Imaginary[i];
            var re2 = GenericOperator<T, T, T>.Multiply(re, re);
            var im2 = GenericOperator<T, T, T>.Multiply(im, im);
            var re2im2 = GenericOperator<T, T, T>.Add(re2, im2);
            return GenericOperator<T, T, T>.Add(sum, re2im2);
        }

        public ComplexVector(Vector<T> real, Vector<T> imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public override string ToString()
            => $"({Real} + i * {Imaginary})";

        public static ComplexVector<T> operator +(ComplexVector<T> a, ComplexVector<T> b)
            => new ComplexVector<T>(a.Real + b.Real, a.Imaginary + b.Imaginary);

        public static ComplexVector<T> operator +(Vector<T> a, ComplexVector<T> b)
            => new ComplexVector<T>(a + b.Real, b.Imaginary);

        public static ComplexVector<T> operator +(ComplexVector<T> a, Vector<T> b)
            => new ComplexVector<T>(a.Real + b, a.Imaginary);

        public static ComplexVector<T> operator -(ComplexVector<T> a, ComplexVector<T> b)
            => new ComplexVector<T>(a.Real - b.Real, a.Imaginary - b.Imaginary);

        public static ComplexVector<T> operator *(ComplexVector<T> a, ComplexVector<T> b)
            => new ComplexVector<T>(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);

        public static ComplexVector<T> operator *(Vector<T> a, ComplexVector<T> b)
            => new ComplexVector<T>(a * b.Real, a * b.Imaginary);

        public static ComplexVector<T> operator *(ComplexVector<T> a, Vector<T> b)
            => new ComplexVector<T>(a.Real * b, a.Imaginary * b);

        public static ComplexVector<T> operator *(T a, ComplexVector<T> b)
            => new ComplexVector<T>(a * b.Real, a * b.Imaginary);

        public static ComplexVector<T> operator *(ComplexVector<T> a, T b)
            => new ComplexVector<T>(a.Real * b, a.Imaginary * b);

    }
}
