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

    internal static MatchResult EvaluateLt<T>(
        this ValuePattern<T>.Lt lt,
        T value,
        string path)
    {
        if (Comparer<T>.Default.Compare(value, lt.Value) < 0)
        {
            return new MatchResult.Success();
        }

        return new MatchResult.Failure([$"{path}: [ValuePattern.Lt] Expected less than {lt.Value}, got {value}"]);
    }

    internal static MatchResult EvaluateGt<T>(
        this ValuePattern<T>.Gt gt,
        T value,
        string path)
    {
        if (Comparer<T>.Default.Compare(value, gt.Value) > 0)
        {
            return new MatchResult.Success();
        }

        return new MatchResult.Failure([$"{path}: [ValuePattern.Gt] Expected greater than {gt.Value}, got {value}"]);
    }

    internal static MatchResult EvaluateBetween<T>(
        this ValuePattern<T>.Between between,
        T value,
        string path)
    {
        if (Comparer<T>.Default.Compare(value, between.Min) >= 0 &&
            Comparer<T>.Default.Compare(value, between.Max) <= 0)
        {
            return new MatchResult.Success();
        }

        return new MatchResult.Failure([$"{path}: [ValuePattern.Between] Expected between {between.Min} and {between.Max}, got {value}"]);
    }
}
