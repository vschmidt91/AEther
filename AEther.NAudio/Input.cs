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
    public abstract class Input : AudioDevice
    {

        public override SampleFormat Format { get; }

        protected readonly IWaveIn Device;

        public Input(IWaveIn device)
        {
            Device = device;
            Device.DataAvailable += Device_DataAvailable;
            Device.RecordingStopped += Device_RecordingStopped;
            Format = ToSampleFormat(device.WaveFormat);
        }

        private void Device_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            OnStopped?.Invoke(this, e.Exception);
        }

        private void Device_DataAvailable(object? sender, WaveInEventArgs evt)
        {
            var data = evt.Buffer.AsMemory(0, evt.BytesRecorded);
            OnDataAvailable?.Invoke(this, data);
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

        public override void Start()
        {
            Device.StartRecording();
        }

        public override void Stop()
        {
            Device.StopRecording();
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Device.Dispose();
        }

    }
}
