using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AEther
{

    public delegate IAsyncEnumerable<To> Pipe<Ti, To>(IAsyncEnumerable<Ti> inputs);

    public static class Pipe
    {

        public static Pipe<Ti, To> Buffer<Ti, To>(this Pipe<Ti, To> pipe, int capacity = -1)
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
            => inputs => pipe2(pipe1(inputs));

    }

}
