namespace PatternMatching.Matchers;

using Dunet;

[Union]
public partial record ValueMatcher<T> : IMatcher<T>
{
    public MatchResult Match(
        T value,
        string path = "")
    {
        throw new NotImplementedException();
    }

    public partial record Exact(
        T Value) : IMatcher<T>;
}

public static class ValueMatcher
{
    public static ValueMatcher<T>.Exact Exact<T>(
        T value)
    {
        return new ValueMatcher<T>.Exact(value);
    }
}