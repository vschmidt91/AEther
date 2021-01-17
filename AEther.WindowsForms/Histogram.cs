using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace AEther.WindowsForms
{
    public abstract class Histogram : IDisposable
    {

        public readonly Texture2D Texture;

        public Histogram(Texture2D texture)
        {
            Texture = texture;
        }

        public abstract int Position { get; }

        public abstract void Add(ReadOnlySpan<float> src);

        public abstract int Update();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Texture.Dispose();
        }

    }
    public abstract class Histogram<T> : Histogram
    {

        public Format Format => Texture.Description.Format;

        public int Width => Texture.Width;
        public int Length => Texture.Height;

        public readonly bool UseMapping;

        readonly byte[] Buffer;
        readonly T[] Slice;
        readonly int Pitch;

        public override int Position => UpdatePosition;

        int UpdatePosition;
        int WritePosition;

        public Histogram(Graphics graphics, Format format, int width, int height, bool useMapping)
            : base(CreateTexture(graphics, format, width, height, useMapping))
        {

            UseMapping = useMapping;

            if(useMapping)
            {
                var map = Texture.Map();
                Pitch = (int)map.Pitch;
                map.Dispose();
            }
            else
            {
                Pitch = width * format.SizeOfInBytes();
            }
            Buffer = new byte[Pitch * height];

            Slice = new T[4 * width];

            UpdatePosition = 0;
            WritePosition = 0;

        }
        static Texture2D CreateTexture(Graphics graphics, Format format, int width, int height, bool useMapping)
        {
            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = useMapping ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = useMapping ? ResourceUsage.Dynamic : ResourceUsage.Default,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
            };
            return graphics.CreateTexture(desc);
        }

        protected abstract T Convert(float value);

        public override void Add(ReadOnlySpan<float> src)
        {

            for(int i = 0; i < src.Length; ++i)
            {
                Slice[i] = Convert(src[i]);
            }
            var dstOffset = WritePosition * Pitch;
            var lengthInBytes = src.Length * Marshal.SizeOf<T>();

            System.Buffer.BlockCopy(Slice, 0, Buffer, dstOffset, lengthInBytes);

            WritePosition = (WritePosition + 1) % Length;


        }

        int GetBandwidth(ResourceRegion region)
            => Format.SizeOfInBytes() * (region.Bottom - region.Top) * (region.Right - region.Left);

        public override int Update()
        {

            var bandwidth = 0;

            IEnumerable<(int, int)> GetIntervals()
            {
                if (0 < WritePosition)
                {
                    if (UpdatePosition + WritePosition < Length)
                    {
                        yield return (UpdatePosition, UpdatePosition + WritePosition);
                    }
                    else
                    {
                        if (UpdatePosition < Length)
                        {
                            yield return (UpdatePosition, Length);
                        }
                        if (Length < UpdatePosition + WritePosition)
                        {
                            yield return (0, UpdatePosition + WritePosition - Length);
                        }
                    }
                }

            }

            if (UseMapping)
            {

                var map = Texture.Map();
                map.Write(Buffer);
                bandwidth += (int)map.Length;
                UpdatePosition = WritePosition;
                map.Dispose();

            }
            else
            {

                foreach (var (from, to) in GetIntervals())
                {
                    var region = new ResourceRegion(0, from, 0, Width, to, 1);
                    Texture.Update(Buffer, 0, region);
                    bandwidth += GetBandwidth(region);
                }

                UpdatePosition = (UpdatePosition + WritePosition) % Length;
                WritePosition = 0;

            }

            return bandwidth;

        }

    }

    public class FloatHistogram : Histogram<float>
    {

        public FloatHistogram(Graphics graphics, int width, int height, bool useMapping)
            : base(graphics, Format.R32G32B32A32_Float, width, height, useMapping)
        { }

        protected override float Convert(float value) => value;

    }

    public class ByteHistogram : Histogram<byte>
    {

        public ByteHistogram(Graphics graphics, int width, int height, bool useMapping)
            : base(graphics, Format.R8G8B8A8_UNorm, width, height, useMapping)
        { }

        protected override byte Convert(float value) => (byte)(255f * value.Clamp(0, 1));

    }

}
