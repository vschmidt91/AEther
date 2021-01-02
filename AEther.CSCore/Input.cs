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
            Device.DataAvailable += Device_DataAvailable;
            Device.Stopped += Device_Stopped;
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

        private void Device_Stopped(object? sender, RecordingStoppedEventArgs evt)
        {
            OnStopped?.Invoke(this, evt.Exception);
        }

        private void Device_DataAvailable(object? sender, DataAvailableEventArgs evt)
        {
            var samples = evt.Data.AsMemory(evt.Offset, evt.ByteCount);
            OnDataAvailable?.Invoke(this, samples);
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
            Device.Dispose();
        }

    }
}
