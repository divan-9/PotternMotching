namespace PotternMotching.Patterns;

/// <summary>
/// Internal extension methods for evaluating value patterns.
/// </summary>
internal static class ValuePatternExtensions
{
    /// <summary>
    /// Evaluates an exact value pattern.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="EqualityComparer{T}.Default"/> to compare the expected and actual values.
    /// </remarks>
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
