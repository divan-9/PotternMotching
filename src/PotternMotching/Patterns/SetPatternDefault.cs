namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;

/// <summary>
/// A wrapper type for set collection patterns with implicit conversions and collection expression support.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
/// <typeparam name="TDefaultItemMatcher">The default pattern type to use for matching individual items.</typeparam>
/// <remarks>
/// <para>
/// This type is used in generated pattern classes for <see cref="HashSet{T}"/> and <see cref="ISet{T}"/> properties.
/// It provides:
/// </para>
/// <list type="bullet">
/// <item><description>Subset matching - All specified patterns must be found in the collection, but order doesn't matter.</description></item>
/// <item><description>Implicit conversions from collection patterns - Allows custom patterns.</description></item>
/// <item><description>Collection expression support via <see cref="CollectionBuilderAttribute"/>.</description></item>
/// </list>
/// <para>
/// Unlike <see cref="SequencePatternDefault{T, TDefaultItemMatcher}"/>, this type uses subset matching
/// which allows the actual collection to contain additional elements and doesn't care about order.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var pattern = new CompanyPattern(
///     Tags: ["technology", "software"]
/// );
/// // Will match a Company with Tags = ["technology", "software", "cloud"]
/// </code>
/// </example>
[CollectionBuilder(typeof(SetPatternDefaultBuilder), "Create")]
public readonly struct SetPatternDefault<T, TDefaultItemMatcher> : IPattern<IEnumerable<T>>
    where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<IEnumerable<T>> innerMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPatternDefault{T, TDefaultItemMatcher}"/> struct.
    /// </summary>
    /// <param name="matcher">The inner collection pattern to use for matching.</param>
    public SetPatternDefault(
        IPattern<IEnumerable<T>> matcher)
    {
        this.innerMatcher = matcher;
    }

    /// <summary>
    /// Creates a subset pattern from a collection of values.
    /// </summary>
    /// <param name="value">The collection of values that must all be found in the target collection.</param>
    /// <returns>A subset pattern that matches collections containing all specified values.</returns>
    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return new SetPatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Subset([..
                value.Select(v => (TDefaultItemMatcher)TDefaultItemMatcher.Create(v))
            ]));
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.innerMatcher.Evaluate(value, path);
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
    /// Implicitly converts a collection pattern to a set pattern default.
    /// </summary>
    /// <param name="pattern">The collection pattern to convert.</param>
    public static implicit operator SetPatternDefault<T, TDefaultItemMatcher>(
        CollectionPattern<T> pattern)
    {
        return new(pattern);
    }
}

/// <summary>
/// Builder class for creating <see cref="SetPatternDefault{T, TDefaultItemMatcher}"/> instances from collection expressions.
/// </summary>
/// <remarks>
/// This class is used by the C# compiler when collection expressions are used with <see cref="SetPatternDefault{T, TDefaultItemMatcher}"/>.
/// </remarks>
public class SetPatternDefaultBuilder
{
    /// <summary>
    /// Creates a set pattern from a span of patterns.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TDefaultItemMatcher">The pattern type for individual items.</typeparam>
    /// <param name="values">The span of patterns to create a subset from.</param>
    /// <returns>A set pattern default.</returns>
    public static SetPatternDefault<T, TDefaultItemMatcher> Create<T, TDefaultItemMatcher>(
        ReadOnlySpan<TDefaultItemMatcher> values)
        where TDefaultItemMatcher : IPattern<T>, IPatternConstructor<T>
    {
        return new SetPatternDefault<T, TDefaultItemMatcher>(
            CollectionPattern.Subset([.. values]));
    }
}