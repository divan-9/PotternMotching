namespace PotternMotching.Patterns;

using PotternMotching.Matchers;

/// <summary>
/// Represents a pattern for matching individual values.
/// </summary>
/// <typeparam name="T">The type of value to match.</typeparam>
/// <remarks>
/// This struct wraps a <see cref="ValueMatcher{T}"/> and tracks whether the pattern has been set.
/// It supports implicit conversion from values for convenient pattern creation.
/// Used in auto-generated pattern classes for simple value properties.
/// </remarks>
public readonly struct ValuePattern<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValuePattern{T}"/>.
    /// </summary>
    /// <param name="matcher">The value matcher to use.</param>
    public ValuePattern(
        ValueMatcher<T>? matcher)
    {
        this.Matcher = matcher;
        this.IsSet = true;
    }

    /// <summary>
    /// Gets the underlying value matcher.
    /// </summary>
    /// <remarks>
    /// This will be null if the pattern was not set (default value).
    /// In pattern matching, null values are treated as wildcards (always match).
    /// </remarks>
    public ValueMatcher<T>? Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has been explicitly set.
    /// </summary>
    public bool IsSet { get; }

    /// <summary>
    /// Implicitly converts a value to an exact match pattern.
    /// </summary>
    /// <param name="value">The value to match exactly.</param>
    /// <returns>A value pattern that matches the specified value exactly.</returns>
    /// <example>
    /// <code>
    /// ValuePattern&lt;int&gt; pattern = 42; // Implicitly creates exact match
    /// </code>
    /// </example>
    public static implicit operator ValuePattern<T>(
        T value)
    {
        return new(new ValueMatcher<T>.Exact(value));
    }
}