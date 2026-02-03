namespace PotternMotching.Patterns;

public readonly struct PatternDefault<T, TPatternDefault> : IPattern<T>
    where TPatternDefault : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<T> innerMatcher;

    public PatternDefault()
    {
        this.innerMatcher = new EmptyPattern<T>();
    }

    public PatternDefault(IPattern<T> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IPattern<T> From(
        T value)
    {
        return new PatternDefault<T, TPatternDefault>(TPatternDefault.Create(value));
    }

    public MatchResult Evaluate(T value, string path = "")
    {
        return this.innerMatcher.Evaluate(value, path);
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