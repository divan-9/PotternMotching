namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;

[CollectionBuilder(typeof(SequencePatternDefaultBuilder), "Create")]
public readonly struct SequencePatternDefault<T, TDefaultItemMatcher> : IPattern<IEnumerable<T>>
    where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<IEnumerable<T>> innerPattern;

    public SequencePatternDefault()
    {
        this.innerPattern = new EmptyPattern<IEnumerable<T>>();
    }

    public SequencePatternDefault(
        IPattern<IEnumerable<T>> innerPattern)
    {
        this.innerPattern = innerPattern;
    }

    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([..
                value.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
            ]));
    }

    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.innerPattern.Evaluate(value, path);
    }

    public IEnumerator<TDefaultItemMatcher> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public static implicit operator SequencePatternDefault<T, TDefaultItemMatcher>(
        CollectionPattern<T> pattern)
    {
        return new(pattern);
    }

    public static implicit operator SequencePatternDefault<T, TDefaultItemMatcher>(
        T[] items)
    {
        return CollectionPattern.Sequence([
            ..items.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
        ]);
    }
}

public class SequencePatternDefaultBuilder
{
    public static SequencePatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([.. values]));
    }

    public static SequencePatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<T> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([
                .. values.ToArray().Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
            ]));
    }
}