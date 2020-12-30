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
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;

namespace AEther.CSCore
{
    public abstract class Input : SampleSource
    {

        public override SampleFormat Format { get; }

        readonly ISoundIn Device;

        public Input(ISoundIn device)
        {

            Device = device;

            //CodecFactory.Instance.GetCodec(@"C:\Users\Ryzen\Google Drive\Projekte\170521.mp3")
            //    .ToSampleSource()
            //    .ToWaveSource()
            //    .WriteToStream(stream);
            //stream.Seek(0, SeekOrigin.Begin);

            Device.Initialize();

            var type = (Device.WaveFormat.WaveFormatTag, Device.WaveFormat.BitsPerSample) switch
            {
                (AudioEncoding.Pcm, 16) => SampleType.UInt16,
                (AudioEncoding.IeeeFloat, 32) => SampleType.Float32,
                (AudioEncoding.Extensible, 32) => SampleType.Float32,
                _ => throw new Exception(),
            };
            Format = new SampleFormat(type, Device.WaveFormat.SampleRate, Device.WaveFormat.Channels);

        }

        public void Playback()
        {

            var output = new WaveOut();
            var source = new SoundInSource(Device);

            output.Initialize(source);
            output.Play();

        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {

            var stopTask = new TaskCompletionSource<bool>();

            async void dataAvailable(object? sender, DataAvailableEventArgs evt)
            {
                var samples = evt.Data.AsMemory(evt.Offset, evt.ByteCount);
                await writer.WriteAsync(samples);
            }

            void stopped(object? sender, RecordingStoppedEventArgs evt)
            {
                if(evt.HasError)
                {
                    stopTask.TrySetException(evt.Exception);
                }
                else
                {
                    stopTask.SetResult(true);
                }
            }

            Device.DataAvailable += dataAvailable;
            Device.Stopped += stopped;

            cancel.Register(Device.Stop);

            Device.Start();

            await stopTask.Task;

            Device.DataAvailable -= dataAvailable;
            Device.Stopped -= stopped;

            await writer.CompleteAsync(stopTask.Task.Exception);

            //while (Input.RecordingState != RecordingState.Stopped)
            //{
            //    await Task.Delay(1);
            //}

        }

        public override void Dispose()
        {
            Device.Dispose();
        }

    }
}
