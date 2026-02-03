namespace PotternMotching.Patterns;

using Dunet;

/// <summary>
/// Provides matchers for individual values.
/// </summary>
/// <typeparam name="T">The type of value to match.</typeparam>
/// <remarks>
/// This is a discriminated union type representing different ways to match individual values.
/// Currently supports exact equality matching.
/// </remarks>
[Union]
public partial record ValuePattern<T> : IPattern<T>, IPatternConstructor<T>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return this.Match(exact => exact.EvaluateExact(value, path));
    }

    public static IPattern<T> Create(
        T value)
    {
        return new Exact(value);
    }

    /// <summary>
    /// Matches a value using exact equality comparison.
    /// </summary>
    /// <param name="Value">The expected value to match against.</param>
    /// <remarks>
    /// Uses <see cref="EqualityComparer{T}.Default"/> for comparison.
    /// </remarks>
    public partial record Exact(
        T Value) : IPattern<T>;

    public static implicit operator PatternDefault<T, Exact>(
        ValuePattern<T> matcher)
    {
        return new PatternDefault<T, Exact>(matcher);
    }
}

public static class ValuePattern
{
    public static ValuePattern<T>.Exact Exact<T>(
        T value)
    {
        return new ValuePattern<T>.Exact(value);
    }
}