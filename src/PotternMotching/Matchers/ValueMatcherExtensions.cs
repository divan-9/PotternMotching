namespace PotternMotching.Matchers;

internal static class ValueMatcherExtensions
{
    internal static MatchResult EvaluateExact<T>(
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
