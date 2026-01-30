namespace PatternMatching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Matchers;

[CollectionBuilder(typeof(SetPattern), "Create")]
public readonly struct SetPattern<T> : IReadOnlyCollection<T>
{
    public SetPattern(
        CollectionMatcher<T>.MatchAll values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public CollectionMatcher<T>.MatchAll Values { get; }

    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public static implicit operator SetPattern<T>(
        T[] values)
    {
        var matchers = values
            .Select(v => ValueMatcher.Exact(v));

        return new(new CollectionMatcher<T>.MatchAll([.. matchers]));
    }

    public static implicit operator SetPattern<T>(
        CollectionMatcher<T>.MatchAll pattern)
    {
        return new(pattern);
    }
}

public static class SetPattern
{
    public static SetPattern<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return values.ToArray();
    }
}