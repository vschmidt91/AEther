using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using CSCore;
using CSCore.Codecs;
using CSCore.SoundIn;
using CSCore.CoreAudioAPI;

using System.Linq;
using CSCore.SoundOut;
using CSCore.Codecs.WAV;

namespace AEther.CSCore
{
    public class Listener : SampleSource, IDisposable
    {

        public override SampleFormat Format { get; }

        private readonly ISoundIn Input;

        public Listener(int? device = null)
        {

            var format = new WaveFormat();
            var priority = ThreadPriority.Normal;
            if(device == null)
            {
                Input = new WasapiLoopbackCapture(0, format, priority);
            }
            else
            {
                using var enumerator = new MMDeviceEnumerator();
                using var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
                var endpointArray = endpoints.ToArray();
                var endpoint = endpointArray[device ?? 0];
                Input = new WasapiCapture(false, AudioClientShareMode.Shared, 0, format, priority)
                {
                    Device = endpoint
                };
            }

            //CodecFactory.Instance.GetCodec(@"C:\Users\Ryzen\Google Drive\Projekte\170521.mp3")
            //    .ToSampleSource()
            //    .ToWaveSource()
            //    .WriteToStream(stream);
            //stream.Seek(0, SeekOrigin.Begin);

            Input.Initialize();

            var type = (Input.WaveFormat.WaveFormatTag, Input.WaveFormat.BitsPerSample) switch
            {
                (AudioEncoding.Pcm, 16) => SampleType.UInt16,
                (AudioEncoding.IeeeFloat, 32) => SampleType.Float32,
                (AudioEncoding.Extensible, 32) => SampleType.Float32,
                _ => throw new Exception(),
            };
            Format = new SampleFormat(type, Input.WaveFormat.SampleRate, Input.WaveFormat.Channels);

        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {

            var stopTask = new TaskCompletionSource<bool>();

            void dataAvailable(object? sender, DataAvailableEventArgs evt)
            {
                var offset = 0;
                while (offset < evt.ByteCount)
                {

                    var target = writer.GetMemory(evt.ByteCount);
                    var count = Math.Min(target.Length, evt.ByteCount - offset);
                    var source = evt.Data.AsMemory(evt.Offset + offset, count);

                    source.CopyTo(target);
                    writer.Advance(count);
                    offset += count;

                    var task = Task.Run(() => writer.FlushAsync());

                }
            }

            void stopped(object? sender, RecordingStoppedEventArgs evt)
            {
                stopTask.SetResult(true);
            }

            Input.DataAvailable += dataAvailable;
            Input.Stopped += stopped;

            cancel.Register(Input.Stop);

            Input.Start();

            await stopTask.Task;

            Input.DataAvailable -= dataAvailable;
            Input.Stopped -= stopped;

            await writer.CompleteAsync();

            while (Input.RecordingState != RecordingState.Stopped)
            {
                await Task.Delay(1);
            }

        }

        public void Dispose()
        {
            Input.Dispose();
        }

    }
}
