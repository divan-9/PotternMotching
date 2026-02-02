namespace PotternMotching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PotternMotching.Matchers;

/// <summary>
/// Represents a pattern for matching ordered sequences (arrays, lists, etc.).
/// </summary>
/// <typeparam name="TItem">The type of items in the sequence.</typeparam>
/// <typeparam name="TItemPattern">The type of pattern used to match individual items.</typeparam>
/// <remarks>
/// This struct wraps a <see cref="CollectionMatcher{T}"/> and supports collection initializer syntax.
/// Used in auto-generated pattern classes for array and list properties.
/// </remarks>
[CollectionBuilder(typeof(SequencePattern), "Create")]
public readonly struct SequencePattern<TItem, TItemPattern> : IEnumerable
    where TItemPattern : IMatcher<TItem>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SequencePattern{TItem, TPattern}"/>.
    /// </summary>
    /// <param name="matcher">The collection matcher to use.</param>
    public SequencePattern(
        CollectionMatcher<TItem> matcher)
    {
        this.Matcher = matcher;
        this.IsSet = true;
    }

    /// <summary>
    /// Gets the underlying collection matcher.
    /// </summary>
    /// <remarks>
    /// This will be null if the pattern was not set (default value).
    /// In pattern matching, null values are treated as wildcards (always match).
    /// </remarks>
    public CollectionMatcher<TItem> Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has been explicitly set.
    /// </summary>
    public bool IsSet { get; }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implicitly converts an array of matchers to a sequence pattern that matches items in exact order.
    /// </summary>
    /// <param name="matchers">The matchers for each item in the expected sequence.</param>
    /// <returns>A sequence pattern that matches the exact sequence of items.</returns>
    /// <example>
    /// <code>
    /// SequencePattern&lt;int, ValueMatcher&lt;int&gt;&gt; pattern = [
    ///     ValueMatcher.Exact(1),
    ///     ValueMatcher.Exact(2)
    /// ];
    /// </code>
    /// </example>
    public static implicit operator SequencePattern<TItem, TItemPattern>(
        TItemPattern[] matchers)
    {
        return new(new CollectionMatcher<TItem>.Sequence([.. matchers]));
    }

    /// <summary>
    /// Implicitly converts a collection matcher to a sequence pattern.
    /// </summary>
    /// <param name="pattern">The collection matcher.</param>
    /// <returns>A sequence pattern wrapping the collection matcher.</returns>
    public static implicit operator SequencePattern<TItem, TItemPattern>(
        CollectionMatcher<TItem> pattern)
    {
        return new(pattern);
    }
}

/// <summary>
/// Provides factory methods for creating sequence patterns.
/// </summary>
public class SequencePattern
{
    /// <summary>
    /// Creates a sequence pattern from a span of matchers (used by collection initializer syntax).
    /// </summary>
    /// <typeparam name="TItem">The type of items in the sequence.</typeparam>
    /// <typeparam name="TItemPattern">The type of pattern used to match individual items.</typeparam>
    /// <param name="values">The matchers for the sequence.</param>
    /// <returns>A sequence pattern.</returns>
    public static SequencePattern<TItem, TItemPattern> Create<TItem, TItemPattern>(
        ReadOnlySpan<TItemPattern> values)
        where TItemPattern : IMatcher<TItem>
    {
        return values.ToArray();
    }
}