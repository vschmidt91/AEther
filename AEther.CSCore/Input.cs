#nullable enable

using CSCore;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using System;

namespace AEther.CSCore
{
    public abstract class Input : SampleSource
    {

        public override SampleFormat Format { get; }

        readonly ISoundIn Device;

        public Input(ISoundIn device)
        {

            Device = device;
            Device.DataAvailable += Device_DataAvailable;
            Device.Stopped += Device_Stopped;
            Device.Initialize();

            SampleType type = (Device.WaveFormat.WaveFormatTag, Device.WaveFormat.BitsPerSample) switch
            {
                (AudioEncoding.Pcm, 16) => SampleType.UInt16.Instance,
                (AudioEncoding.IeeeFloat, 32) => SampleType.Float32.Instance,
                (AudioEncoding.Extensible, 32) => SampleType.Float32.Instance,
                _ => throw new Exception(),
            };
            Format = new SampleFormat(type, Device.WaveFormat.SampleRate, Device.WaveFormat.Channels);

        }

        private void Device_Stopped(object? sender, RecordingStoppedEventArgs evt)
        {
            Stopped?.Invoke(this, evt.Exception);
        }

        private void Device_DataAvailable(object? sender, DataAvailableEventArgs evt)
        {
            var samples = evt.Data.AsMemory(evt.Offset, evt.ByteCount);
            DataAvailable?.Invoke(this, samples);
        }

        public void Playback()
        {
            var output = new WaveOut();
            var source = new SoundInSource(Device);
            output.Initialize(source);
            output.Play();
        }

        public override void Start()
        {
            Device.Start();
        }

        public override void Stop()
        {
            Device.Stop();
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            Device.Dispose();
        }

    }
}
