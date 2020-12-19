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

        int Channel;

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

        public void Dispose()
        {
            ManagedBass.Bass.Free();
        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {

            byte[]? buffer = null;

            bool Procedure(int handle, IntPtr data, int length, IntPtr user)
            {

                if (buffer == default || buffer.Length < length)
                {
                    buffer = new byte[length];
                }

                Marshal.Copy(data, buffer, 0, length);

                var offset = 0;
                while (offset < length)
                {

                    var target = writer.GetMemory(length);
                    var count = Math.Min(target.Length, length - offset);
                    var source = buffer.AsMemory(0, count);

                    source.CopyTo(target);
                    writer.Advance(count);
                    offset += count;

                }

                return !cancel.IsCancellationRequested;

            }

            var stopTask = new TaskCompletionSource<bool>();

            cancel.Register(() => ManagedBass.Bass.ChannelStop(Channel));

            var flags = BassFlags.RecordPause;
            Channel = ManagedBass.Bass.RecordStart(Format.SampleRate, Format.ChannelCount, flags, Procedure);
            ManagedBass.Bass.ChannelPlay(Channel);

            await stopTask.Task;

            await writer.CompleteAsync();

        }

    }
}
