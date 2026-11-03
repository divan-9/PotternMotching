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
        return this.Match(
            exact => exact.EvaluateExact(value, path),
            lt => lt.EvaluateLt(value, path),
            gt => gt.EvaluateGt(value, path),
            between => between.EvaluateBetween(value, path));
    }

    /// <summary>
    /// Creates an exact value pattern from the specified value.
    /// </summary>
    /// <param name="value">The expected value.</param>
    /// <returns>A pattern that matches the specified value using equality comparison.</returns>
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

    /// <summary>
    /// Matches values that are less than the specified value.
    /// </summary>
    /// <param name="Value">The exclusive upper bound.</param>
    public partial record Lt(
        T Value) : IPattern<T>;

    /// <summary>
    /// Matches values that are greater than the specified value.
    /// </summary>
    /// <param name="Value">The exclusive lower bound.</param>
    public partial record Gt(
        T Value) : IPattern<T>;

    /// <summary>
    /// Matches values that are between the specified bounds, inclusive.
    /// </summary>
    /// <param name="Min">The inclusive lower bound.</param>
    /// <param name="Max">The inclusive upper bound.</param>
    public partial record Between(
        T Min,
        T Max) : IPattern<T>;

    /// <summary>
    /// Converts a value pattern to a default pattern wrapper.
    /// </summary>
    /// <param name="matcher">The value pattern to wrap.</param>
    public static implicit operator PatternDefault<T, Exact>(
        ValuePattern<T> matcher)
    {
        return new PatternDefault<T, Exact>(matcher);
    }
}

/// <summary>
/// Factory methods for creating value patterns.
/// </summary>
public static class ValuePattern
{
    /// <summary>
    /// Marker value used to request exact null matching in generated pattern defaults.
    /// </summary>
    public readonly record struct NullPatternToken;

    /// <summary>
    /// Creates a pattern that matches a value using equality comparison.
    /// </summary>
    /// <typeparam name="T">The type of value to match.</typeparam>
    /// <param name="value">The expected value.</param>
    /// <returns>An exact value pattern.</returns>
    public static ValuePattern<T>.Exact Exact<T>(
        T value)
    {
        return new ValuePattern<T>.Exact(value);
    }

    /// <summary>
    /// Creates a pattern that matches values less than the specified value.
    /// </summary>
    /// <typeparam name="T">The numeric type to match.</typeparam>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>A less-than value pattern.</returns>
    public static ValuePattern<T>.Lt Lt<T>(
        T value)
        where T : System.Numerics.INumber<T>
    {
        return new ValuePattern<T>.Lt(value);
    }

    /// <summary>
    /// Creates a pattern that matches values greater than the specified value.
    /// </summary>
    /// <typeparam name="T">The numeric type to match.</typeparam>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>A greater-than value pattern.</returns>
    public static ValuePattern<T>.Gt Gt<T>(
        T value)
        where T : System.Numerics.INumber<T>
    {
        return new ValuePattern<T>.Gt(value);
    }

    /// <summary>
    /// Creates a pattern that matches values between the specified bounds, inclusive.
    /// </summary>
    /// <typeparam name="T">The numeric type to match.</typeparam>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>A between value pattern.</returns>
    public static ValuePattern<T>.Between Between<T>(
        T min,
        T max)
        where T : System.Numerics.INumber<T>
    {
        return new ValuePattern<T>.Between(min, max);
    }

    /// <summary>
    /// Creates a marker that requests exact null matching in generated pattern defaults.
    /// </summary>
    /// <returns>A null pattern marker.</returns>
    public static NullPatternToken Null()
    {
        return default;
    }
}