namespace PotternMotching.Matchers;

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
public partial record ValueMatcher<T> : IMatcher<T>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return this.Match(exact => exact.EvaluateExact(value, path));
    }

    /// <summary>
    /// Matches a value using exact equality comparison.
    /// </summary>
    /// <param name="Value">The expected value to match against.</param>
    /// <remarks>
    /// Uses <see cref="EqualityComparer{T}.Default"/> for comparison.
    /// </remarks>
    public partial record Exact(
        T Value) : IMatcher<T>;

    public static implicit operator DefaultMatcher<T>(
        ValueMatcher<T> matcher)
    {
        return new DefaultMatcher<T>(matcher);
    }
}

public static class ValueMatcher
{
    public static ValueMatcher<T>.Exact Exact<T>(
        T value)
    {
        return new ValueMatcher<T>.Exact(value);
    }
}