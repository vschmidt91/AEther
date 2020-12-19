using System;
using System.Collections.Generic;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AEther
{
    public abstract class SampleSource
    {

        public abstract SampleFormat Format { get; }

        public abstract Task WriteToAsync(PipeWriter writer, CancellationToken cancel = default);

        public async IAsyncEnumerable<PipeHandle> ReadAllAsync([EnumeratorCancellation] CancellationToken cancel = default)
        {

            var pipe = new System.IO.Pipelines.Pipe();
            var writerTask = WriteToAsync(pipe.Writer, cancel);

            var inputs = pipe.Reader.ReadAllAsync();
            await foreach (var input in inputs)
            {
                yield return input;
            }

            await writerTask;

        }


    }
}
