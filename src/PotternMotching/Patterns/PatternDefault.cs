namespace PotternMotching.Patterns;

public readonly struct PatternDefault<T, TPatternDefault> : IPattern<T>, IPatternConstructor<T>
    where TPatternDefault : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<T>? innerMatcher;

    public PatternDefault(IPattern<T> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IPattern<T> From(
        T value)
    {
        return new PatternDefault<T, TPatternDefault>(TPatternDefault.Create(value));
    }

    public static IPattern<T> Create(
        T value)
    {
        return From(value);
    }

    public MatchResult Evaluate(T value, string path = "")
    {
        return this.innerMatcher?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    public static implicit operator PatternDefault<T, TPatternDefault>(
        T value)
    {
        return new PatternDefault<T, TPatternDefault>(TPatternDefault.Create(value));
    }

    public static implicit operator PatternDefault<T, TPatternDefault>(
        TPatternDefault value)
    {
        return new PatternDefault<T, TPatternDefault>(value);
    }
}