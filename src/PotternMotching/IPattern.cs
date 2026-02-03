namespace PotternMotching;

/// <summary>
/// Defines a pattern that can evaluate whether a value matches a specific pattern.
/// </summary>
/// <typeparam name="T">The type of value to match against.</typeparam>
/// <remarks>
/// Patterns are the core abstraction in PotternMotching. They evaluate values and return
/// either <see cref="MatchResult.Success"/> if the value matches the pattern, or
/// <see cref="MatchResult.Failure"/> with detailed reasons if it doesn't.
/// </remarks>
public interface IPattern<in T>
{
    /// <summary>
    /// Evaluates whether the given value matches this pattern.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="path">
    /// The path to this value in the object graph, used for generating detailed error messages.
    /// Defaults to an empty string for the root object.
    /// </param>
    /// <returns>
    /// A <see cref="MatchResult.Success"/> if the value matches, or a <see cref="MatchResult.Failure"/>
    /// with detailed reasons if it doesn't.
    /// </returns>
    MatchResult Evaluate(
         T value,
         string path = "");
}

/// <summary>
/// Defines a static interface for types that can construct patterns from values.
/// </summary>
/// <typeparam name="T">The type of value to create patterns for.</typeparam>
/// <remarks>
/// This interface is used by the default pattern wrapper types to enable implicit conversions
/// from values to patterns. Implementations provide a static factory method to create patterns.
/// </remarks>
public interface IPatternConstructor<in T>
{
    /// <summary>
    /// Creates a pattern from a value.
    /// </summary>
    /// <param name="value">The value to create a pattern from.</param>
    /// <returns>A pattern that matches the specified value.</returns>
    static abstract IPattern<T> Create(T value);
}