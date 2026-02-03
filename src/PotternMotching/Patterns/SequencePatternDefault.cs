namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;

[CollectionBuilder(typeof(SequencePatternDefaultBuilder), "Create")]
public readonly struct SequencePatternDefault<T, TDefaultItemMatcher> : IPattern<IEnumerable<T>>
    where TDefaultItemMatcher : IPattern<T>
{
    private readonly IPattern<IEnumerable<T>> innerMatcher;

    public SequencePatternDefault()
    {
        this.innerMatcher = new EmptyPattern<IEnumerable<T>>();
    }

    public SequencePatternDefault(
        IPattern<IEnumerable<T>> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([..
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

public class SequencePatternDefaultBuilder
{
    public static SequencePatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IPattern<T>
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([.. values]));
    }
}