using ManagedBass;
using System;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO.Pipelines;

namespace AEther.Bass
{
    public class Listener : SampleSource, IDisposable
    {

        public override SampleFormat Format { get; }
        public int BufferSize { get; } = 1 << 10;

        int Channel;
        byte[] Buffer = Array.Empty<byte>();

        public Listener(int? deviceIndex = default)
        {


            ManagedBass.Bass.RecordInit(deviceIndex ?? -1);
            Format = new SampleFormat(SampleType.UInt16, 44100, 2);

        }

        public static DeviceInfo[] GetDevices()
        {
            var deviceCount = ManagedBass.Bass.RecordingDeviceCount;
            var devices = Enumerable.Range(0, deviceCount)
                .Select(ManagedBass.Bass.RecordGetDeviceInfo)
                .ToArray();
            return devices;
        }

        bool Procedure(int handle, IntPtr data, int length, IntPtr user)
        {

            if (Buffer.Length < length)
            {
                Buffer = new byte[length];
            }

            Marshal.Copy(data, Buffer, 0, length);

            OnDataAvailable?.Invoke(this, Buffer.AsMemory(0, length));

            return true;

        }

        public override void Start()
        {
            Buffer = new byte[BufferSize];
            var flags = BassFlags.RecordPause;
            Channel = ManagedBass.Bass.RecordStart(Format.SampleRate, Format.ChannelCount, flags, Procedure);
            ManagedBass.Bass.ChannelPlay(Channel);
        }
        public override void Stop()
        {
            ManagedBass.Bass.ChannelStop(Channel);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            ManagedBass.Bass.Free();
        }

    }
}
