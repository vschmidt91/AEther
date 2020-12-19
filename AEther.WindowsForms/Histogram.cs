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
    public interface IHistogram : IDisposable
    {

        int Position { get; }

        Texture2D Texture { get; }

        void Add(ReadOnlySpan<float> src);

        int Update(DeviceContext context);

    }
    public abstract class Histogram<T> : IHistogram
    {

        public Format Format => Texture.Description.Format;

        public int Width => Texture.Width;
        public int Length => Texture.Height;

        public Texture2D Texture { get; }

        public readonly bool UseMapping;

        readonly byte[] Buffer;
        readonly T[] Slice;

        public int Position => UpdatePosition;

        int UpdatePosition;
        int WritePosition;

        public Histogram(SharpDX.Direct3D11.Device device, Format format, int width, int height, bool? useMapping = default)
        {

            UseMapping = useMapping ?? false;

            Texture = new Texture2D(new SharpDX.Direct3D11.Texture2D(device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = UseMapping ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = UseMapping ? ResourceUsage.Dynamic : ResourceUsage.Default,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
            }));

            Buffer = new byte[Texture.RowPitch * height];
            Slice = new T[4 * width];

            UpdatePosition = 0;
            WritePosition = 0;

        }

        protected abstract T Convert(float value);

        public void Add(ReadOnlySpan<float> src)
        {

            for(int i = 0; i < src.Length; ++i)
            {
                Slice[i] = Convert(src[i]);
            }
            var dstOffset = WritePosition * Texture.RowPitch;
            var lengthInBytes = src.Length * Marshal.SizeOf<T>();

            System.Buffer.BlockCopy(Slice, 0, Buffer, dstOffset, lengthInBytes);

            WritePosition = (WritePosition + 1) % Length;


        }

        int GetBandwidth(ResourceRegion region)
            => Format.SizeOfInBytes() * (region.Bottom - region.Top) * (region.Right - region.Left);

        public int Update(DeviceContext context)
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

                using (var map = Texture.Map(context))
                {
                    if (Buffer?.Length != map.Length)
                    {
                        throw new Exception();
                    }
                    map.Write(Buffer);
                    bandwidth += (int)map.Length;
                }
                UpdatePosition = WritePosition;

            }
            else
            {

                foreach (var (from, to) in GetIntervals())
                {
                    var region = new ResourceRegion(0, from, 0, Width, to, 1);
                    Texture.Update(context, Buffer, 0, region);
                    bandwidth += GetBandwidth(region);
                }

                UpdatePosition = (UpdatePosition + WritePosition) % Length;
                WritePosition = 0;

            }

            return bandwidth;

        }

        public void Dispose()
        {
            Texture.Dispose();
        }

    }

    public class FloatHistogram : Histogram<float>
    {

        public FloatHistogram(SharpDX.Direct3D11.Device device, int width, int height, bool? useMapping = default)
            : base(device, Format.R32G32B32A32_Float, width, height, useMapping)
        { }

        protected override float Convert(float value) => value;

    }

    public class ByteHistogram : Histogram<byte>
    {

        public ByteHistogram(SharpDX.Direct3D11.Device device, int width, int height, bool? useMapping = default)
            : base(device, Format.R8G8B8A8_UNorm, width, height, useMapping)
        { }

        protected override byte Convert(float value) => (byte)(255f * value.Clamp(0, 1));

    }

}
