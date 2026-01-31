namespace PotternMotching;

/// <summary>
/// Defines a matcher that can evaluate whether a value matches a specific pattern.
/// </summary>
/// <typeparam name="T">The type of value to match against.</typeparam>
/// <remarks>
/// Matchers are the core abstraction in PotternMotching. They evaluate values and return
/// either <see cref="MatchResult.Success"/> if the value matches the pattern, or
/// <see cref="MatchResult.Failure"/> with detailed reasons if it doesn't.
/// </remarks>
public interface IMatcher<in T>
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