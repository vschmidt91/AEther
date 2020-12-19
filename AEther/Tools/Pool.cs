using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public class Pool<T>
    {

        readonly Func<T> Factory;
        readonly ConcurrentBag<T> Buffer = new ConcurrentBag<T>();

        public Pool(Func<T> factory)
        {
            Factory = factory;
        }

        public T Get()
            => TryGet(out T item) ? item : Factory();

        public bool TryGet(out T item)
            => Buffer.TryTake(out item);

        public void Add(T item)
            => Buffer.Add(item);

        public void Create()
            => Add(Factory());

    }
}
