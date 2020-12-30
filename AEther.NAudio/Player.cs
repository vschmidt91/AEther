using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Dmo;
using NAudio.Wave;

namespace AEther.NAudio
{
    public class Player : SampleSource, IDisposable
    {

        public override SampleFormat Format { get; }

        readonly WaveStream Input;

        public Player(string path)
        {
            var file = new FileInfo(path);
            Input = file.Extension switch
            {
                ".wav" => new WaveFileReader(path),
                ".mp3" => new Mp3FileReader(path),
                _ => throw new Exception(),
            };
            Format = NAudio.Input.ToSampleFormat(Input.WaveFormat);
        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {

            while(!cancel.IsCancellationRequested)
            {

                var target = writer.GetMemory();
                var count = await Input.ReadAsync(target, cancel);

                if (count == 0)
                {
                    break;
                }

                writer.Advance(count);

                var result = writer.FlushAsync(cancel);
                if(result.IsCompleted)
                {
                    break;
                }

            }

            await writer.CompleteAsync();

        }

        public override void Dispose()
        {
            Input.Dispose();
        }

    }
}
