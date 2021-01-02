using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AEther
{

    public delegate IAsyncEnumerable<To> Pipe<Ti, To>(IAsyncEnumerable<Ti> inputs)
        where Ti : struct
        where To : struct;

    public readonly struct PipeHandle
    {

        public readonly ReadOnlySequence<byte> Data;
        public readonly PipeReader Reader;

        public PipeHandle(ReadOnlySequence<byte> data, PipeReader reader)
        {
            Data = data;
            Reader = reader;
        }

    }

    public static class Pipe
    {

        public static async IAsyncEnumerable<PipeHandle> ReadAllAsync(this PipeReader reader, CancellationToken cancel = default)
        {
            while (true)
            {
                var result = await reader.ReadAsync(cancel);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
                yield return new PipeHandle(result.Buffer, reader);
            }
        }

        public static Pipe<Ti, To> Buffer<Ti, To>(this Pipe<Ti, To> pipe, int capacity = -1)
            where Ti : struct
            where To : struct
        {
            Channel<To> buffer;
            if (capacity == -1)
            {
                buffer = Channel.CreateUnbounded<To>(new UnboundedChannelOptions
                {
                    AllowSynchronousContinuations = true,
                    SingleReader = true,
                    SingleWriter = true,
                });
            }
            else
            {
                buffer = Channel.CreateBounded<To>(new BoundedChannelOptions(capacity)
                {
                    AllowSynchronousContinuations = true,
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = true,
                });
            }
            async IAsyncEnumerable<To> RunAsync(IAsyncEnumerable<Ti> inputs)
            {
                var task = Task.Run(async () =>
                {
                    await foreach (var output in pipe(inputs))
                    {
                        await buffer.Writer.WriteAsync(output);
                    }
                    buffer.Writer.Complete();
                });
                await foreach (var output in buffer.Reader.ReadAllAsync())
                {
                    yield return output;
                }
                await task;
            }
            return RunAsync;
        }

        public static Pipe<Ti, To> Chain<Ti, Tm, To>(this Pipe<Ti, Tm> pipe1, Pipe<Tm, To> pipe2)
            where Ti : struct
            where Tm : struct
            where To : struct
            => inputs => pipe2(pipe1(inputs));

    }

}
