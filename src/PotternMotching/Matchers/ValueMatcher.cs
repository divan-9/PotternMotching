namespace PotternMotching.Matchers;

using Dunet;

[Union]
public partial record ValueMatcher<T> : IMatcher<T>
{
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return this.Match(exact => exact.EvaluateExact(value, path));
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