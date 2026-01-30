namespace PatternMatching.PatternBuilders;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Patterns;

[CollectionBuilder(typeof(SetPatternBuilder), "Create")]
public readonly struct SetPatternBuilder<T> : IReadOnlyCollection<T>
{
    public SetPatternBuilder(
        CollectionPattern<T>.MatchAll values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public CollectionPattern<T>.MatchAll Values { get; }

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

    public static implicit operator SetPatternBuilder<T>(
        HashSet<T> values)
    {
        return new(new CollectionPattern<T>.MatchAll(values));
    }

    public static implicit operator SetPatternBuilder<T>(
        CollectionPattern<T>.MatchAll pattern)
    {
        return new(pattern);
    }
}

internal static class SetPatternBuilder
{
    public static SetPatternBuilder<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return new SetPatternBuilder<T>(
            new CollectionPattern<T>.MatchAll([.. values]));
    }
}