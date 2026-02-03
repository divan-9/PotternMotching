namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;

[CollectionBuilder(typeof(SetPatternDefaultBuilder), "Create")]
public readonly struct SetPatternDefault<T, TDefaultItemMatcher> : IPattern<IEnumerable<T>>
    where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<IEnumerable<T>> innerMatcher;

    public SetPatternDefault()
    {
        this.innerMatcher = new EmptyPattern<IEnumerable<T>>();
    }

    public SetPatternDefault(
        IPattern<IEnumerable<T>> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new SetPatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.AnyOrder([..
                value.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
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

    public static implicit operator SetPatternDefault<T, TDefaultItemMatcher>(
        CollectionPattern<T> pattern)
    {
        return new(pattern);
    }
}

public class SetPatternDefaultBuilder
{
    public static SetPatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SetPatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.AnyOrder([.. values]));
    }
}