﻿using NAudio.Wave;
using System;

namespace AEther.NAudio
{
    public abstract class Input : SampleSource
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
            Stopped?.Invoke(this, e.Exception);
        }

        private void Device_DataAvailable(object? sender, WaveInEventArgs evt)
        {
            var data = evt.Buffer.AsMemory(0, evt.BytesRecorded);
            DataAvailable?.Invoke(this, data);
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
            SampleType type = (format.Encoding, format.BitsPerSample) switch
            {
                (WaveFormatEncoding.Pcm, 16) => SampleType.UInt16.Instance,
                (WaveFormatEncoding.IeeeFloat, 32) => SampleType.Float32.Instance,
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
            Device.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
