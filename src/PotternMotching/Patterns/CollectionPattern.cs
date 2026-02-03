namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;
using Dunet;

/// <summary>
/// Pattern for matching collections of values.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
/// <remarks>
/// This is a discriminated union type representing different ways to match collections:
/// <list type="bullet">
/// <item><see cref="AnyElement"/> - Matches if any element in the collection satisfies a pattern.</item>
/// <item><see cref="Sequence"/> - Matches collections with exact sequence of elements in order.</item>
/// <item><see cref="Subset"/> - Matches if all specified patterns can be found in the collection in any order.</item>
/// <item><see cref="StartsWith"/> - Matches if the collection starts with the specified patterns.</item>
/// <item><see cref="EndsWith"/> - Matches if the collection ends with the specified patterns.</item>
/// </list>
/// </remarks>
[Union]
public partial record CollectionPattern<T> : IPattern<IEnumerable<T>>
{
    /// <summary>
    /// Creates a sequence pattern from a collection of values.
    /// </summary>
    /// <param name="value">The collection of values to match exactly.</param>
    /// <returns>A sequence pattern that matches the exact collection in order.</returns>
    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return CollectionPattern.Sequence(
            [.. value.Select(v => new ValuePattern<T>.Exact(v))]);
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.Match(
            anyElement => anyElement.EvaluateAnyElement(value, path),
            sequence => sequence.EvaluateSequence(value, path),
            anyOrder => anyOrder.EvaluateSubset(value, path),
            startsWith => startsWith.EvaluateStartsWith(value, path),
            endsWith => endsWith.EvaluateEndsWith(value, path));
    }

    /// <summary>
    /// Matches if any element in the collection satisfies the specified pattern.
    /// </summary>
    /// <param name="Pattern">The pattern that at least one element must match.</param>
    public partial record AnyElement(
        IPattern<T> Pattern)
    {
    };

    /// <summary>
    /// Matches collections with an exact sequence of elements in order.
    /// The collection must have exactly the same length and each element must match the corresponding pattern.
    /// </summary>
    /// <param name="Patterns">The array of patterns that must match sequentially.</param>
    public partial record Sequence(
        IPattern<T>[] Patterns);

    /// <summary>
    /// Matches if all specified patterns can be found in the collection in any order.
    /// Each pattern must match at least one element, but the collection can contain additional elements.
    /// </summary>
    /// <param name="Patterns">The array of patterns that must all be found in the collection.</param>
    public partial record Subset(
        IPattern<T>[] Patterns);

    /// <summary>
    /// Matches if the collection starts with the specified patterns in order.
    /// The collection can have additional elements after the starting patterns.
    /// </summary>
    /// <param name="Patterns">The array of patterns that must match at the start of the collection.</param>
    public partial record StartsWith(
        IPattern<T>[] Patterns);

    /// <summary>
    /// Matches if the collection ends with the specified patterns in order.
    /// The collection can have additional elements before the ending patterns.
    /// </summary>
    /// <param name="Patterns">The array of patterns that must match at the end of the collection.</param>
    public partial record EndsWith(
        IPattern<T>[] Patterns);
}

/// <summary>
/// Factory methods for creating collection patterns.
/// </summary>
public static class CollectionPattern
{
    /// <summary>
    /// Creates a sequence pattern from an array of patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="example">The array of patterns that must match sequentially.</param>
    /// <returns>A sequence pattern.</returns>
    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.Sequence Sequence<T>(
        IPattern<T>[] example)
    {
        return new CollectionPattern<T>.Sequence(example);
    }

    /// <summary>
    /// Creates a sequence pattern from an array of values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="example">The array of values that must match exactly in order.</param>
    /// <returns>A sequence pattern with exact value matchers.</returns>
    public static CollectionPattern<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionPattern<T>.Sequence([.. example.Select(ValuePattern.Exact)]);
    }

    /// <summary>
    /// Creates a subset pattern from an array of patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="example">The array of patterns that must all be found in the collection.</param>
    /// <returns>A subset pattern.</returns>
    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.Subset Subset<T>(
        IPattern<T>[] example)
    {
        return new CollectionPattern<T>.Subset(example);
    }

    /// <summary>
    /// Creates a subset pattern from an array of values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="example">The array of values that must all be found in the collection.</param>
    /// <returns>A subset pattern with exact value matchers.</returns>
    public static CollectionPattern<T>.Subset Subset<T>(
        T[] example)
    {
        return new CollectionPattern<T>.Subset([.. example.Select(ValuePattern.Exact)]);
    }

    /// <summary>
    /// Creates a pattern that matches if any element satisfies the specified pattern.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="pattern">The pattern that at least one element must match.</param>
    /// <returns>An any-element pattern.</returns>
    public static CollectionPattern<T>.AnyElement AnyElement<T>(
        IPattern<T> pattern)
    {
        return new CollectionPattern<T>.AnyElement(pattern);
    }

    /// <summary>
    /// Creates a pattern that matches if any element equals the specified value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="value">The value that at least one element must equal.</param>
    /// <returns>An any-element pattern with an exact value matcher.</returns>
    public static CollectionPattern<T>.AnyElement AnyElement<T>(
        T value)
    {
        return new CollectionPattern<T>.AnyElement(ValuePattern.Exact(value));
    }

    /// <summary>
    /// Creates a pattern that matches if the collection starts with the specified patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="patterns">The patterns that must match at the start of the collection.</param>
    /// <returns>A starts-with pattern.</returns>
    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.StartsWith StartsWith<T>(
        IPattern<T>[] patterns)
    {
        return new CollectionPattern<T>.StartsWith(patterns);
    }

    /// <summary>
    /// Creates a pattern that matches if the collection starts with the specified values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The values that must match exactly at the start of the collection.</param>
    /// <returns>A starts-with pattern with exact value matchers.</returns>
    public static CollectionPattern<T>.StartsWith StartsWith<T>(
        T[] values)
    {
        return new CollectionPattern<T>.StartsWith([.. values.Select(ValuePattern.Exact)]);
    }

    /// <summary>
    /// Creates a pattern that matches if the collection ends with the specified patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="patterns">The patterns that must match at the end of the collection.</param>
    /// <returns>An ends-with pattern.</returns>
    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.EndsWith EndsWith<T>(
        IPattern<T>[] patterns)
    {
        return new CollectionPattern<T>.EndsWith(patterns);
    }

    /// <summary>
    /// Creates a pattern that matches if the collection ends with the specified values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The values that must match exactly at the end of the collection.</param>
    /// <returns>An ends-with pattern with exact value matchers.</returns>
    public static CollectionPattern<T>.EndsWith EndsWith<T>(
        T[] values)
    {
        return new CollectionPattern<T>.EndsWith([.. values.Select(ValuePattern.Exact)]);
    }
}