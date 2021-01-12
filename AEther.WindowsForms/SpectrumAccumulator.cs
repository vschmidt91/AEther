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

        public abstract void Add(ReadOnlySpan<float> values);

        public abstract int Update(DeviceContext context);

        public void Dispose()
        {
            Texture.Dispose();
        }

    }

    public abstract class SpectrumAccumulator<T> : SpectrumAccumulator
        where T : struct 
    {

        readonly T[] Buffer;

        protected SpectrumAccumulator(Device device, int length, SharpDX.DXGI.Format format)
            : base(CreateTexture(device, length, format))
        {
            Buffer = new T[4 * length];
        }

        static Texture2D CreateTexture(Device device, int length, SharpDX.DXGI.Format format)
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
            return new Texture2D(new SharpDX.Direct3D11.Texture2D(device, desc));
        }

        public override void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        protected abstract T Convert(float value);

        protected abstract T Combine(T src, T dst);

        public override void Add(ReadOnlySpan<float> src)
        {
            for (int i = 0; i < Buffer.Length; ++i)
            {
                var value = Convert(src[i]);
                Buffer[i] = Combine(value, Buffer[i]);
            }
        }

        public override int Update(DeviceContext context)
        {
            var map = Texture.Map(context);
            map.WriteRange(Buffer, 0, Buffer.Length);
            map.Dispose();
            return Buffer.Length * Marshal.SizeOf<T>();
        }

    }

    public class ByteSpectrum : SpectrumAccumulator<byte>
    {

        public ByteSpectrum(Device device, int length)
            : base(device, length, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
        { }

        protected override byte Combine(byte src, byte dst)
            => Math.Max(src, dst);

        protected override byte Convert(float value)
            => (byte)(255 * value.Clamp(0, 1));

    }

    public class FloatSpectrum : SpectrumAccumulator<float>
    {

        public FloatSpectrum(Device device, int length)
            : base(device, length, SharpDX.DXGI.Format.R32G32B32A32_Float)
        { }

        protected override float Combine(float src, float dst)
            => Math.Max(src, dst);

        protected override float Convert(float value)
            => value;

    }

}
