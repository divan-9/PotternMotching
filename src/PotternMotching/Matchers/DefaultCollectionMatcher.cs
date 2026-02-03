namespace PotternMotching.Matchers;

using System.Runtime.CompilerServices;

// TODO: create similar matcher for sets which would use CollectionMatcher.AnyOrder in collection expression
[CollectionBuilder(typeof(DefaultCollectionMatcherBuilder), "Create")]
public readonly struct DefaultCollectionMatcher<T, TDefaultItemMatcher> : IMatcher<IEnumerable<T>>
    where TDefaultItemMatcher : IMatcher<T>
{
    private readonly IMatcher<IEnumerable<T>> innerMatcher;

    public DefaultCollectionMatcher()
    {
        this.innerMatcher = new EmptyMatcher<IEnumerable<T>>();
    }

    public DefaultCollectionMatcher(
        IMatcher<IEnumerable<T>> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IMatcher<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new DefaultCollectionMatcher<T, TDefaultItemMatcher>(
            CollectionMatcher.Sequence([..
                value.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.From(v))
            ]));
    }

    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.innerMatcher.Evaluate(value, path);
    }

    public IEnumerator<TDefaultItemMatcher> GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class DefaultCollectionMatcherBuilder
{
    public static DefaultCollectionMatcher<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IMatcher<T>
    {
        return new DefaultCollectionMatcher<T, TDefaultItemMatcher>(
            CollectionMatcher.Sequence([.. values]));
    }
}