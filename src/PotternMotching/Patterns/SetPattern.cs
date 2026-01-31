namespace PotternMotching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PotternMotching.Matchers;

/// <summary>
/// Represents a pattern for matching unordered sets of values.
/// </summary>
/// <typeparam name="T">The type of items in the set.</typeparam>
/// <remarks>
/// This struct wraps a <see cref="CollectionMatcher{T}.MatchAll"/> matcher and supports collection initializer syntax.
/// Unlike <see cref="SequencePattern{TItem, TPattern}"/>, this pattern matches items in any order.
/// Used in auto-generated pattern classes for HashSet and ISet properties.
/// </remarks>
[CollectionBuilder(typeof(SetPattern), "Create")]
public readonly struct SetPattern<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SetPattern{T}"/>.
    /// </summary>
    /// <param name="matcher">The MatchAll matcher to use.</param>
    public SetPattern(
        CollectionMatcher<T>.MatchAll matcher)
    {
        this.Matcher = matcher;
        this.IsSet = true;
    }

    /// <summary>
    /// Gets the underlying MatchAll collection matcher.
    /// </summary>
    /// <remarks>
    /// This will be null if the pattern was not set (default value).
    /// In pattern matching, null values are treated as wildcards (always match).
    /// </remarks>
    public CollectionMatcher<T>.MatchAll Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has been explicitly set.
    /// </summary>
    public bool IsSet { get; }

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">This property is not implemented.</exception>
    public int Count => throw new NotImplementedException();

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Implicitly converts an array of values to a set pattern that matches all values in any order.
    /// </summary>
    /// <param name="values">The values that must all be present in the set.</param>
    /// <returns>A set pattern that matches if all values are found.</returns>
    /// <example>
    /// <code>
    /// SetPattern&lt;string&gt; pattern = ["apple", "banana"];
    /// // Matches any set containing both "apple" and "banana"
    /// </code>
    /// </example>
    public static implicit operator SetPattern<T>(
        T[] values)
    {
        var matchers = values
            .Select(v => ValueMatcher.Exact(v));

        return new(new CollectionMatcher<T>.MatchAll([.. matchers]));
    }

    /// <summary>
    /// Implicitly converts a MatchAll collection matcher to a set pattern.
    /// </summary>
    /// <param name="pattern">The MatchAll matcher.</param>
    /// <returns>A set pattern wrapping the matcher.</returns>
    public static implicit operator SetPattern<T>(
        CollectionMatcher<T>.MatchAll pattern)
    {
        return new(pattern);
    }
}

/// <summary>
/// Provides factory methods for creating set patterns.
/// </summary>
public static class SetPattern
{
    /// <summary>
    /// Creates a set pattern from a span of values (used by collection initializer syntax).
    /// </summary>
    /// <typeparam name="T">The type of items in the set.</typeparam>
    /// <param name="values">The values that must all be present.</param>
    /// <returns>A set pattern.</returns>
    public static SetPattern<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return values.ToArray();
    }
}