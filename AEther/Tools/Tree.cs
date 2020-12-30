using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{

    public class Tree<T>
    {

        public readonly T Item;
        public readonly IList<Tree<T>> Children;

        public Tree(T item = default, params Tree<T>[] children)
            : this(item, children.ToList())
        { }

        public Tree(T item , IEnumerable<Tree<T>> children)
        {
            Item = item;
            Children = children as IList<Tree<T>> ?? children.ToList();
        }

        public static Tree<T> Unfold<S>(S seed, Func<S, (T, IEnumerable<S>)> f)
        {
            var (item, childSeeds) = f(seed);
            return new Tree<T>(item, childSeeds.Select(s => Tree<T>.Unfold(s, f)).ToArray());
        }

        public S Fold<S>(Func<T, IEnumerable<S>, S> f)
            => f(Item, Children.Select(c => c.Fold(f)));

        public Tree<S> Map<S>(Func<T, S> f)
            => new Tree<S>(f(Item), Children.Select(c => c.Map(f)));

        public Tree<(T, Tree<T>)> WithParents(Tree<T> parent = null)
            => new Tree<(T, Tree<T>)>(
                (Item, parent),
                Children.Select(d => d.WithParents(this)));

        public Tree<(T, int)> WithLevel(int level = 0)
            => new Tree<(T, int)>(
                (Item, level),
                Children.Select(d => d.WithLevel(level + 1)));

        public IEnumerable<T> FlattenPreOrder()
            => new[] { Item }
            .Concat(Children.SelectMany(c => c.FlattenPreOrder()));

        public IEnumerable<T> FlattenPostOrder()
            => Children.SelectMany(c => c.FlattenPostOrder())
            .Concat(new[] { Item });

        public IEnumerable<T> FlattenLevel()
            => WithLevel()
            .FlattenPreOrder()
            .OrderBy(p => p.Item2)
            .Select(p => p.Item1);

        public override string ToString()
            => Children.Any()
            ? $"{Item} [{string.Join(", ", Children)}]"
            : Item?.ToString() ?? string.Empty;

    }

    public static class RecursionExt
    {


        public static Sequence<T> UnfoldToSequence<T, S>(this S seed, Func<S, (T, S)?> f)
        {
            var x = f(seed);
            if (!x.HasValue)
                return Sequence<T>.Empty.Instance;
            var (item, tailSeed) = x.Value;
            return new Sequence<T>.Cons(item, tailSeed.UnfoldToSequence(f));
        }

    }

}
