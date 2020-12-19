using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace AEther.WPF
{
    public class Histogram
    {

        public static readonly PixelFormat Format = PixelFormats.Rgba128Float;

        public readonly float[] Input;

        public readonly int Width;
        public readonly int Height;

        int Stride => 4 * Width;
        int Position;

        public Histogram(int width, int height)
        {
            Width = width;
            Height = height;
            Input = new float[4 * Width * Height];
            Position = 0;
        }

        public void Add(ReadOnlySpan<float> src)
        {

            var offset = Position * Stride;
            var dst = Input.AsSpan(offset);
            src.CopyTo(dst);

            Position += 1;
            if (Position == Height)
                Position = 0;

        }

        public void Update(WriteableBitmap dst)
        {

            int offset = Position * Stride;

            dst.Lock();
            dst.WritePixels(new Int32Rect(0, 0, Width, Height - Position), Input, dst.BackBufferStride, offset);
            dst.WritePixels(new Int32Rect(0, Height - Position, Width, Position), Input, dst.BackBufferStride, 0);
            dst.Unlock();

        }

    }
}
