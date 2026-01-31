namespace PotternMotching.Matchers;

using System.Runtime.CompilerServices;
using Dunet;

/// <summary>
/// Provides matchers for collections of values.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>
/// This is a discriminated union type representing different ways to match collections:
/// <list type="bullet">
/// <item><see cref="MatchAll"/> - Matches if all specified items are found in any order.</item>
/// <item><see cref="Sequence"/> - Matches if the collection has exactly the specified items in order.</item>
/// <item><see cref="EndsWith"/> - Matches if the collection ends with the specified items.</item>
/// <item><see cref="StartsWith"/> - Matches if the collection starts with the specified items.</item>
/// </list>
/// </remarks>
[Union]
public partial record CollectionMatcher<T> : IMatcher<IEnumerable<T>>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.Match(
            matchAll => matchAll.EvaluateMatchAll(value, path),
            sequence => sequence.EvaluateSequence(value, path),
            endsWith => endsWith.EvaluateEndsWith(value, path),
            startsWith => startsWith.EvaluateStartsWith(value, path)
        );
    }

    /// <summary>
    /// Matches if all specified items are found in the collection, regardless of order.
    /// </summary>
    /// <param name="items">The matchers for items that must all be present.</param>
    /// <remarks>
    /// This is useful for matching unordered collections like sets or when you don't care about order.
    /// Each pattern must match at least one item in the collection. The collection may contain additional items.
    /// </remarks>
    public partial record MatchAll(
        IMatcher<T>[] items);

    /// <summary>
    /// Matches if the collection contains exactly the specified items in the exact order.
    /// </summary>
    /// <param name="items">The matchers for the expected sequence of items.</param>
    /// <remarks>
    /// Both the length and order of items must match exactly. The collection cannot have more or fewer items.
    /// </remarks>
    public partial record Sequence(
        IMatcher<T>[] items);

    /// <summary>
    /// Matches if the collection ends with the specified items in order.
    /// </summary>
    /// <param name="items">The matchers for items expected at the end of the collection.</param>
    /// <remarks>
    /// The collection may have additional items before the matched suffix.
    /// </remarks>
    public partial record EndsWith(
        IMatcher<T>[] items);

    /// <summary>
    /// Matches if the collection starts with the specified items in order.
    /// </summary>
    /// <param name="items">The matchers for items expected at the start of the collection.</param>
    /// <remarks>
    /// The collection may have additional items after the matched prefix.
    /// </remarks>
    public partial record StartsWith(
        IMatcher<T>[] items);
}

/// <summary>
/// Provides factory methods for creating collection matchers.
/// </summary>
public static class CollectionMatcher
{
    /// <summary>
    /// Creates a matcher that succeeds if all specified matchers match items in the collection (in any order).
    /// Notice that the same item in the collection can satisfy multiple matchers.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="patterns">The matchers that must all find matching items.</param>
    /// <returns>A MatchAll collection matcher.</returns>
    /// <example>
    /// <code>
    /// var matcher = CollectionMatcher.MatchAll([
    ///     ValueMatcher.Exact("apple"),
    ///     ValueMatcher.Exact("banana")
    /// ]);
    /// matcher.Evaluate(["banana", "cherry", "apple"]); // Success
    /// matcher.Evaluate(["apple"]); // Failure - missing "banana"
    /// </code>
    /// </example>
    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.MatchAll MatchAll<T>(
        IMatcher<T>[] patterns)
    {
        return new CollectionMatcher<T>.MatchAll(patterns);
    }

    /// <summary>
    /// Creates a matcher that succeeds if all specified values are present in the collection (in any order).
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="patterns">The exact values that must all be present.</param>
    /// <returns>A MatchAll collection matcher.</returns>
    /// <example>
    /// <code>
    /// var matcher = CollectionMatcher.MatchAll(["apple", "banana"]);
    /// matcher.Evaluate(["banana", "cherry", "apple"]); // Success
    /// </code>
    /// </example>
    public static CollectionMatcher<T>.MatchAll MatchAll<T>(
        T[] patterns)
    {
        return new CollectionMatcher<T>.MatchAll([.. patterns.Select(ValueMatcher.Exact)]);
    }

    /// <summary>
    /// Creates a matcher that succeeds if the collection contains exactly the specified items in order.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="example">The matchers for the expected sequence.</param>
    /// <returns>A Sequence collection matcher.</returns>
    /// <example>
    /// <code>
    /// var matcher = CollectionMatcher.Sequence([
    ///     ValueMatcher.Exact("a"),
    ///     ValueMatcher.Exact("b")
    /// ]);
    /// matcher.Evaluate(["a", "b"]); // Success
    /// matcher.Evaluate(["a", "b", "c"]); // Failure - too many items
    /// matcher.Evaluate(["b", "a"]); // Failure - wrong order
    /// </code>
    /// </example>
    public static CollectionMatcher<T>.Sequence Sequence<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.Sequence(example);
    }

    /// <summary>
    /// Creates a matcher that succeeds if the collection ends with the specified items in order.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="example">The matchers for items expected at the end.</param>
    /// <returns>An EndsWith collection matcher.</returns>
    /// <example>
    /// <code>
    /// var matcher = CollectionMatcher.EndsWith([
    ///     ValueMatcher.Exact("x"),
    ///     ValueMatcher.Exact("y")
    /// ]);
    /// matcher.Evaluate(["a", "b", "x", "y"]); // Success
    /// matcher.Evaluate(["x", "y"]); // Success
    /// matcher.Evaluate(["a", "b"]); // Failure
    /// </code>
    /// </example>
    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.EndsWith EndsWith<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.EndsWith(example);
    }

    /// <summary>
    /// Creates a matcher that succeeds if the collection ends with the specified exact values in order.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="example">The exact values expected at the end.</param>
    /// <returns>An EndsWith collection matcher.</returns>
    public static CollectionMatcher<T>.EndsWith EndsWith<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.EndsWith([.. example.Select(ValueMatcher.Exact)]);
    }

    /// <summary>
    /// Creates a matcher that succeeds if the collection starts with the specified items in order.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="example">The matchers for items expected at the start.</param>
    /// <returns>A StartsWith collection matcher.</returns>
    /// <example>
    /// <code>
    /// var matcher = CollectionMatcher.StartsWith([
    ///     ValueMatcher.Exact("a"),
    ///     ValueMatcher.Exact("b")
    /// ]);
    /// matcher.Evaluate(["a", "b", "x", "y"]); // Success
    /// matcher.Evaluate(["a", "b"]); // Success
    /// matcher.Evaluate(["x", "y"]); // Failure
    /// </code>
    /// </example>
    public static CollectionMatcher<T>.StartsWith StartsWith<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.StartsWith(example);
    }
}