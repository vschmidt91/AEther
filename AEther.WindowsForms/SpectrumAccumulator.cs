using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{

    public abstract class SpectrumAccumulator
    {

        public readonly Texture2D Texture;

        public SpectrumAccumulator(Texture2D texture)
        {
            Texture = texture;
        }

        public abstract void Clear();

        public abstract void Add(ReadOnlySpan<double> values);

        public abstract int Update();

        public void Dispose()
        {
            Texture.Dispose();
        }

    }

    public abstract class SpectrumAccumulator<T> : SpectrumAccumulator
        where T : struct 
    {

        public readonly T[] Buffer;

        protected SpectrumAccumulator(Graphics graphics, int length, SharpDX.DXGI.Format format)
            : base(CreateTexture(graphics, length, format))
        {
            Buffer = new T[4 * length];
        }

        static Texture2D CreateTexture(Graphics graphics, int length, SharpDX.DXGI.Format format)
        {
            var desc = new Texture2DDescription
            {
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = format,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Dynamic,
                Width = length,
                Height = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                MipLevels = 1,
                ArraySize = 1,
            };
            return graphics.CreateTexture(desc);
        }

        public override void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        protected abstract T Convert(double value);

        protected abstract T Combine(T src, T dst);

        public override void Add(ReadOnlySpan<double> src)
        {
            if(src.Length < Buffer.Length)
            {
                throw new Exception();
            }
            for (int i = 0; i < Buffer.Length; ++i)
            {
                var value = Convert(src[i]);
                Buffer[i] = Combine(value, Buffer[i]);
            }
        }

        public override int Update()
        {
            var map = Texture.Map();
            map.WriteRange(Buffer, 0, Buffer.Length);
            map.Dispose();
            return Buffer.Length * Marshal.SizeOf<T>();
        }

    }

    public class ByteSpectrum : SpectrumAccumulator<byte>
    {

        public ByteSpectrum(Graphics graphics, int length)
            : base(graphics, length, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
        { }

        protected override byte Combine(byte src, byte dst)
            => Math.Max(src, dst);

        protected override byte Convert(double value)
            => (byte)(255 * value.Clamp(0, 1));

    }

    public class FloatSpectrum : SpectrumAccumulator<float>
    {

        public FloatSpectrum(Graphics graphics, int length)
            : base(graphics, length, SharpDX.DXGI.Format.R32G32B32A32_Float)
        { }

        protected override float Combine(float src, float dst)
            => Math.Max(src, dst);

        protected override float Convert(double value)
            => (float)value;

    }

}
