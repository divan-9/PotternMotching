namespace PotternMotching.Matchers;

public readonly struct DefaultMatcher<T, TDefaultMatcher> : IMatcher<T>
    where TDefaultMatcher : IMatcher<T>
{
    private readonly IMatcher<T> innerMatcher;

    public DefaultMatcher()
    {
        this.innerMatcher = new EmptyMatcher<T>();
    }

    public DefaultMatcher(IMatcher<T> matcher)
    {
        this.innerMatcher = matcher;
    }

    public static IMatcher<T> From(
        T value)
    {
        return new DefaultMatcher<T, TDefaultMatcher>(TDefaultMatcher.From(value));
    }

    public MatchResult Evaluate(T value, string path = "")
    {
        return this.innerMatcher.Evaluate(value, path);
    }

    public static implicit operator DefaultMatcher<T, TDefaultMatcher>(
        T value)
    {
        return new DefaultMatcher<T, TDefaultMatcher>(TDefaultMatcher.From(value));
    }

    public static implicit operator DefaultMatcher<T, TDefaultMatcher>(
        TDefaultMatcher value)
    {
        return new DefaultMatcher<T, TDefaultMatcher>(value);
    }
}