namespace PotternMotching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PotternMotching.Matchers;

/// <summary>
/// Represents a pattern for matching unordered sets of values.
/// </summary>
/// <typeparam name="TItem">The type of items in the set.</typeparam>
/// <typeparam name="TItemPattern">The type of pattern used to match individual items.</typeparam>
/// <remarks>
/// This struct wraps a <see cref="CollectionMatcher{T}.MatchAll"/> matcher and supports collection initializer syntax.
/// Unlike <see cref="SequencePattern{TItem, TPattern}"/>, this pattern matches items in any order.
/// Used in auto-generated pattern classes for HashSet and ISet properties.
/// </remarks>
[CollectionBuilder(typeof(SetPattern), "Create")]
public readonly struct SetPattern<TItem, TItemPattern> : IEnumerable<TItem>
    where TItemPattern : IMatcher<TItem>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SetPattern{TItem, TPattern}"/>.
    /// </summary>
    /// <param name="matcher">The MatchAll matcher to use.</param>
    public SetPattern(
        CollectionMatcher<TItem>.MatchAll matcher)
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
    public CollectionMatcher<TItem>.MatchAll Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has been explicitly set.
    /// </summary>
    public bool IsSet { get; }

    public IEnumerator<TItem> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implicitly converts a MatchAll collection matcher to a set pattern.
    /// </summary>
    /// <param name="pattern">The MatchAll matcher.</param>
    /// <returns>A set pattern wrapping the matcher.</returns>
    public static implicit operator SetPattern<TItem, TItemPattern>(
        CollectionMatcher<TItem>.MatchAll pattern)
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
    /// <typeparam name="TItem">The type of items in the set.</typeparam>
    /// <typeparam name="TItemPattern">The type of items matchers in the set.</typeparam>
    /// <param name="patterns">The values that must all be present.</param>
    /// <returns>A set pattern.</returns>
    public static SetPattern<TItem, TItemPattern> Create<TItem, TItemPattern>(
        ReadOnlySpan<TItemPattern> patterns)
        where TItemPattern : IMatcher<TItem>
    {
        return new CollectionMatcher<TItem>.MatchAll([.. patterns]);
    }

    public static SetPattern<TItem, TItemPattern> FromValues<TItem, TItemPattern>(
        ReadOnlySpan<TItem> patterns)
        where TItemPattern : IMatcher<TItem>
    {
        return new(new CollectionMatcher<TItem>.MatchAll([.. patterns]));
    }
}