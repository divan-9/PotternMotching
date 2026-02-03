namespace PotternMotching.Patterns;

internal static class ValuePatternExtensions
{
    internal static MatchResult EvaluateExact<T>(
        this ValuePattern<T>.Exact exact,
        T value,
        string path)
    {
        if (EqualityComparer<T>.Default.Equals(exact.Value, value))
        {
            return new MatchResult.Success();
        }

        return new MatchResult.Failure([$"{path}: [ValuePattern.Exact] Expected {exact.Value}, got {value}"]);
    }
}
