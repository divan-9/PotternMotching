namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;

/// <summary>
/// A wrapper type for sequence collection patterns with implicit conversions and collection expression support.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
/// <typeparam name="TDefaultItemMatcher">The default pattern type to use for matching individual items.</typeparam>
/// <remarks>
/// <para>
/// This type is used in generated pattern classes for array and list properties. It provides:
/// </para>
/// <list type="bullet">
/// <item><description>Default behavior - When not specified, matches any collection (returns Success).</description></item>
/// <item><description>Implicit conversions from arrays - An array is converted to a sequence pattern.</description></item>
/// <item><description>Implicit conversions from collection patterns - Allows custom patterns.</description></item>
/// <item><description>Collection expression support via <see cref="CollectionBuilderAttribute"/>.</description></item>
/// </list>
/// <para>
/// Sequence patterns match collections in exact order with the same length.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var pattern = new CompanyPattern(
///     Branches: [
///         new AddressPattern(Zip: "98101"),
///         new AddressPattern(Zip: "98102")
///     ]
/// );
/// </code>
/// </example>
[CollectionBuilder(typeof(SequencePatternDefaultBuilder), "Create")]
public readonly struct SequencePatternDefault<T, TDefaultItemMatcher> : IPattern<IEnumerable<T>>
    where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<IEnumerable<T>>? innerPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequencePatternDefault{T, TDefaultItemMatcher}"/> struct.
    /// </summary>
    /// <param name="innerPattern">The inner collection pattern to use for matching.</param>
    public SequencePatternDefault(
        IPattern<IEnumerable<T>> innerPattern)
    {
        this.innerPattern = innerPattern;
    }

    /// <summary>
    /// Creates a sequence pattern from a collection of values.
    /// </summary>
    /// <param name="value">The collection of values to match exactly in order.</param>
    /// <returns>A sequence pattern that matches the exact collection.</returns>
    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([..
                value.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
            ]));
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.innerPattern?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    /// <summary>
    /// Gets an enumerator for the collection builder support.
    /// </summary>
    /// <returns>An enumerator (not implemented, used for type inference only).</returns>
    /// <exception cref="NotImplementedException">This method is not intended to be called.</exception>
    public IEnumerator<TDefaultItemMatcher> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Implicitly converts a collection pattern to a sequence pattern default.
    /// </summary>
    /// <param name="pattern">The collection pattern to convert.</param>
    public static implicit operator SequencePatternDefault<T, TDefaultItemMatcher>(
        CollectionPattern<T> pattern)
    {
        return new(pattern);
    }

    /// <summary>
    /// Implicitly converts an array to a sequence pattern default.
    /// </summary>
    /// <param name="items">The array of items to convert to a sequence pattern.</param>
    public static implicit operator SequencePatternDefault<T, TDefaultItemMatcher>(
        T[] items)
    {
        return CollectionPattern.Sequence([
            ..items.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
        ]);
    }
}

/// <summary>
/// Builder class for creating <see cref="SequencePatternDefault{T, TDefaultItemMatcher}"/> instances from collection expressions.
/// </summary>
/// <remarks>
/// This class is used by the C# compiler when collection expressions are used with <see cref="SequencePatternDefault{T, TDefaultItemMatcher}"/>.
/// </remarks>
public class SequencePatternDefaultBuilder
{
    /// <summary>
    /// Creates a sequence pattern from a span of patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TDefaultItemMatcher">The pattern type for individual items.</typeparam>
    /// <param name="values">The span of patterns to create a sequence from.</param>
    /// <returns>A sequence pattern default.</returns>
    public static SequencePatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([.. values]));
    }

    /// <summary>
    /// Creates a sequence pattern from a span of values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TDefaultItemMatcher">The pattern type for individual items.</typeparam>
    /// <param name="values">The span of values to create patterns from.</param>
    /// <returns>A sequence pattern default.</returns>
    public static SequencePatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<T> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SequencePatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Sequence([
                .. values.ToArray().Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
            ]));
    }
}