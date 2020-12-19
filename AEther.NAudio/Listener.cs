using System;
using System.Diagnostics;
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

namespace AEther.NAudio
{
    public abstract class Listener : SampleSource, IDisposable
    {

        public override SampleFormat Format { get; }

        readonly IWaveIn Input;

        public Listener(IWaveIn input)
        {
            Input = input;
            Format = ToSampleFormat(input.WaveFormat);
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

            void dataAvailable(object? sender, WaveInEventArgs evt)
            {
                var offset = 0;
                while(offset < evt.BytesRecorded)
                {

                    var target = writer.GetMemory(evt.BytesRecorded);
                    var count = Math.Min(target.Length, evt.BytesRecorded - offset);
                    var source = evt.Buffer.AsSpan(offset, count);

                    source.CopyTo(target.Span);
                    writer.Advance(count);
                    writer.FlushAsync();

                    offset += count;

                }
            }

            var stopTask = new TaskCompletionSource<bool>();
            void stopped(object? sender, StoppedEventArgs evt)
            {
                stopTask.SetResult(true);
            }

            Input.DataAvailable += dataAvailable;
            Input.RecordingStopped += stopped;

            cancel.Register(Input.StopRecording);

            Input.StartRecording();

            await stopTask.Task;

            Input.DataAvailable -= dataAvailable;
            Input.RecordingStopped -= stopped;

            await writer.CompleteAsync();

        }

        public void Dispose()
        {
            Input.Dispose();
        }

    }
}
