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

public static class ValueMatcherExtensions
{
    public static MatchResult EvaluateExact<T>(
        this ValueMatcher<T>.Exact exact,
        T value,
        string path)
    {
        if (EqualityComparer<T>.Default.Equals(exact.Value, value))
        {
            return new MatchResult.Success();
        }

        return new MatchResult.Failure([$"{path}: [ValueMatcher.Exact] Expected {exact.Value}, got {value}"]);
    }
}