using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEther
{
    public abstract class Sequence<T> : IEnumerable<T>
    {

        private Sequence()
        { }

        public abstract Sequence<S> Map<S>(Func<T, S> f);
        public abstract S Fold<S>(Func<T, S, S> f, S seed = default);
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public sealed class Empty : Sequence<T>
        {

            public static readonly Empty Instance
                = new Empty();

            public override Sequence<S> Map<S>(Func<T, S> f)
                => Sequence<S>.Empty.Instance;

            public override S Fold<S>(Func<T, S, S> f, S seed = default)
                => seed;

            public override string ToString()
                => string.Empty;

            public override IEnumerator<T> GetEnumerator()
                => Enumerable.Empty<T>().GetEnumerator();

        }

        public sealed class Cons : Sequence<T>
        {

            public readonly T Item;
            public readonly Sequence<T> Tail;

            public Cons(T item = default, Sequence<T> tail = null)
            {
                Item = item;
                Tail = tail;
            }

            public override Sequence<S> Map<S>(Func<T, S> f)
                => new Sequence<S>.Cons(f(Item), Tail.Map(f));

            public override S Fold<S>(Func<T, S, S> f, S seed = default)
                => f(Item, Tail.Fold(f, seed));

            public override string ToString()
                => $"{Item} {Tail}";

            public override IEnumerator<T> GetEnumerator()
                => new[] { Item }
                    .Concat(Tail)
                    .GetEnumerator();

        }

    }
}
