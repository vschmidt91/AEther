using System;
using System.Collections.Generic;
using System.Linq;

namespace AEther
{

    public record Tree<T>
    (
        T Item,
        Tree<T>[] Children
    )
    {

        public static Tree<T> Unfold<S>(S seed, Func<S, (T, IEnumerable<S>)> f)
        {
            var (item, childSeeds) = f(seed);
            return new Tree<T>(item, childSeeds.Select(s => Tree<T>.Unfold(s, f)).ToArray());
        }

        public S Fold<S>(Func<T, IEnumerable<S>, S> f)
            => f(Item, Children.Select(c => c.Fold(f)));

        public Tree<S> Map<S>(Func<T, S> f)
            => new(f(Item), Children.Select(c => c.Map(f)).ToArray());

        public Tree<(T, Tree<T>?)> WithParents(Tree<T>? parent = null)
            => new(
                (Item, parent),
                Children.Select(d => d.WithParents(this)).ToArray());

        public Tree<(T, int)> WithLevel(int level = 0)
            => new(
                (Item, level),
                Children.Select(d => d.WithLevel(level + 1)).ToArray());

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
            ? $"{Item} [{string.Join(", ", (IEnumerable<Tree<T>>)Children)}]"
            : Item?.ToString() ?? string.Empty;

    }

}
