namespace PotternMotching.Matchers;

public readonly struct DefaultMatcher<T> : IMatcher<T>
{
    private readonly IMatcher<T> innerMatcher;

    public DefaultMatcher()
    {
        this.innerMatcher = new EmptyMatcher<T>();
    }

    public DefaultMatcher(
        IMatcher<T> matcher)
    {
        this.innerMatcher = matcher;
    }

    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return this.innerMatcher.Evaluate(value, path);
    }

    public static implicit operator DefaultMatcher<T>(
        T value)
    {
        return new DefaultMatcher<T>(ValueMatcher.Exact(value));
    }
}