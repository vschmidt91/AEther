using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{

    public interface ISpectrumAccumulator : IDisposable
    {

        Texture2D Texture { get; }

        void Clear();

        void Add(ReadOnlySpan<float> src);

        int Update(DeviceContext context);

    }

    public abstract class SpectrumAccumulator<T> : ISpectrumAccumulator
        where T : struct 
    {

        public Texture2D Texture { get; }

        readonly T[] Buffer;

        protected SpectrumAccumulator(Device device, int length, SharpDX.DXGI.Format format)
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
            Buffer = new T[4 * length];
            Texture = new Texture2D(new SharpDX.Direct3D11.Texture2D(device, desc));
        }

        public void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        protected abstract T Combine(float src, T dst);

        public void Add(ReadOnlySpan<float> src)
        {
            for (int i = 0; i < Buffer.Length; ++i)
            {
                Buffer[i] = Combine(src[i], Buffer[i]);
            }
        }

        public int Update(DeviceContext context)
        {
            using var map = Texture.Map(context);
            map.WriteRange(Buffer.AsSpan());
            return Buffer.Length * Marshal.SizeOf<T>();
        }

        public void Dispose()
        {
            Texture.Dispose();
        }

    }

    public class ByteSpectrum : SpectrumAccumulator<byte>
    {

        public ByteSpectrum(Device device, int length)
            : base(device, length, SharpDX.DXGI.Format.R8G8B8A8_UNorm)
        { }

        protected override byte Combine(float src, byte dst) => Math.Max(dst, (byte)(255 * src.Clamp(0, 1)));

    }

    public class FloatSpectrum : SpectrumAccumulator<float>
    {

        public FloatSpectrum(Device device, int length)
            : base(device, length, SharpDX.DXGI.Format.R32G32B32A32_Float)
        { }

        protected override float Combine(float src, float dst) => Math.Max(dst, src);

    }

}
