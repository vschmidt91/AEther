using System;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace AEther
{
    public class SampleReader : SampleSource
    {

        public override SampleFormat Format { get; }

        readonly Stream Input;

        public SampleReader(Stream input)
        {
            Input = input;
            Format = WAVHeader.FromStream(input).GetSampleFormat();
        }

        public override async Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default)
        {
            while (!cancel.IsCancellationRequested)
            {
                var target = writer.GetMemory();
                var count = await Input.ReadAsync(target, cancel);
                if (count == 0)
                {
                    break;
                }
                else
                {
                    writer.Advance(count);
                }
            }
            await writer.CompleteAsync();
        }

    }
}
