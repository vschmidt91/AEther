using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AEther
{
    public abstract class Processor<Ti, To>
    {

        readonly ConcurrentBag<Thread> Threads = new ConcurrentBag<Thread>();

        protected abstract IAsyncEnumerable<To> ProcessAsync(Ti input);

        public Thread StartThread(IAsyncEnumerable<Ti> input, EventHandler<To> callback)
        {
            var thread = new Thread(async () =>
            {
                await foreach (var o in RunAsync(input))
                {
                    callback(this, o);
                }
            });
            thread.Start();
            Threads.Add(thread);
            return thread;
        }

        public void Join(TimeSpan? timeout = default)
        {
            foreach(var thread in Threads)
            {
                thread.Join(timeout ?? TimeSpan.Zero);
            }
            Threads.Clear();
        }

        public async IAsyncEnumerable<To> RunAsync(IAsyncEnumerable<Ti> input)
        {
            await foreach(var i in input)
            {
                await foreach (var o in ProcessAsync(i))
                {
                    yield return o;
                }
            }
            yield break;
        }

    }
}
