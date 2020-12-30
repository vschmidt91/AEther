using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using NAudio;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System.Buffers;

namespace AEther.NAudio
{
    public abstract class Input : SampleSource
    {

        public override SampleFormat Format { get; }

        readonly IWaveIn Device;

        public Input(IWaveIn device)
        {
            Device = device;
            Format = ToSampleFormat(device.WaveFormat);
        }

        public void Playback()
        {
            var wo = new WaveOut();
            var bwp = new BufferedWaveProvider(Device.WaveFormat)
            {
                DiscardOnBufferOverflow = true,
            };
            Device.DataAvailable += (sender, evt) =>
            {
                bwp.AddSamples(evt.Buffer, 0, evt.BytesRecorded);
            };
            Device.RecordingStopped += (sender, evt) =>
            {
                wo.Stop();
                wo.Dispose();
            };

            wo.Init(bwp);
            wo.Play();
        }

        public static SampleFormat ToSampleFormat(WaveFormat format)
        {
            var type = (format.Encoding, format.BitsPerSample) switch
            {
                (WaveFormatEncoding.Pcm, 16) => SampleType.UInt16,
                (WaveFormatEncoding.IeeeFloat, 32) => SampleType.Float32,
                _ => throw new Exception(),
            };
            return new SampleFormat(type, format.SampleRate, format.Channels);
        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {

            async void dataAvailable(object? sender, WaveInEventArgs evt)
            {
                await writer.WriteAsync(evt.Buffer.AsMemory(0, evt.BytesRecorded));
            }

            var stopTask = new TaskCompletionSource<bool>();
            void stopped(object? sender, StoppedEventArgs evt)
            {
                stopTask.SetResult(true);
            }

            Device.DataAvailable += dataAvailable;
            Device.RecordingStopped += stopped;

            cancel.Register(Device.StopRecording);

            Device.StartRecording();

            await stopTask.Task;

            Device.DataAvailable -= dataAvailable;
            Device.RecordingStopped -= stopped;

            await writer.CompleteAsync();

        }

        public override void Dispose()
        {
            Device.Dispose();
        }

    }
}
