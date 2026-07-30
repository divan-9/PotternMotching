namespace PotternMotching.Patterns;

/// <summary>
/// A wrapper type for read-only dictionary patterns with implicit conversions.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <typeparam name="TDefaultValuePattern">The default pattern type to use for matching individual values.</typeparam>
/// <remarks>
/// This type is used in generated pattern classes for <see cref="IReadOnlyDictionary{TKey, TValue}"/>
/// properties. It mirrors <see cref="DictionaryPatternDefault{TKey, TValue, TDefaultValuePattern}"/>
/// but evaluates against the read-only dictionary interface.
/// </remarks>
public readonly struct ReadOnlyDictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>
    : IPattern<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
    where TDefaultValuePattern : IPattern<TValue>, IPatternConstructor<TValue>
{
    private readonly IPattern<IReadOnlyDictionary<TKey, TValue>>? innerPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyDictionaryPatternDefault{TKey, TValue, TDefaultValuePattern}"/> struct.
    /// </summary>
    /// <param name="pattern">The inner dictionary pattern to use for matching.</param>
    public ReadOnlyDictionaryPatternDefault(
        IPattern<IReadOnlyDictionary<TKey, TValue>> pattern)
    {
        this.innerPattern = pattern;
    }

    /// <inheritdoc />
    public MatchResult Evaluate(
        IReadOnlyDictionary<TKey, TValue> value,
        string path = "")
    {
        return this.innerPattern?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    /// <summary>
    /// Creates a dictionary pattern from a read-only dictionary of values.
    /// </summary>
    /// <param name="value">The dictionary of values to match.</param>
    /// <returns>A dictionary pattern that matches all specified key-value pairs.</returns>
    public static IPattern<IReadOnlyDictionary<TKey, TValue>> From(
        IReadOnlyDictionary<TKey, TValue> value)
    {
        return new ReadOnlyDictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
            DictionaryPattern.Items(
                value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => TDefaultValuePattern.Create(kvp.Value))));
    }

    /// <summary>
    /// Implicitly converts a dictionary pattern to a read-only dictionary pattern default.
    /// </summary>
    /// <param name="pattern">The dictionary pattern to convert.</param>
    public static implicit operator ReadOnlyDictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
        DictionaryPattern<TKey, TValue> pattern)
    {
        return new ReadOnlyDictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(pattern);
    }
}
