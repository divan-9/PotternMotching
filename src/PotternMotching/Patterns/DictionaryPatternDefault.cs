namespace PotternMotching.Patterns;

/// <summary>
/// A wrapper type for dictionary patterns with implicit conversions.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <typeparam name="TDefaultValuePattern">The default pattern type to use for matching individual values.</typeparam>
/// <remarks>
/// <para>
/// This type is used in generated pattern classes for <see cref="Dictionary{TKey, TValue}"/> and
/// <see cref="IDictionary{TKey, TValue}"/> properties. It provides:
/// </para>
/// <list type="bullet">
/// <item><description>Default behavior - When not specified, matches any dictionary (returns Success).</description></item>
/// <item><description>Implicit conversions from dictionary patterns - Allows custom patterns.</description></item>
/// <item><description>Indexer support for potential future enhancements.</description></item>
/// </list>
/// <para>
/// The default matching strategy uses <see cref="DictionaryPattern{TKey, TValue}.Items"/> which allows
/// the actual dictionary to contain additional keys beyond those specified in the pattern.
/// </para>
/// </remarks>
public readonly struct DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>
    : IPattern<IDictionary<TKey, TValue>>
    where TKey : notnull
    where TDefaultValuePattern : IPattern<TValue>, IPatternConstructor<TValue>
{
    private readonly IPattern<IDictionary<TKey, TValue>>? innerPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryPatternDefault{TKey, TValue, TDefaultValuePattern}"/> struct.
    /// </summary>
    /// <param name="pattern">The inner dictionary pattern to use for matching.</param>
    public DictionaryPatternDefault(
        IPattern<IDictionary<TKey, TValue>> pattern)
    {
        this.innerPattern = pattern;
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        IDictionary<TKey, TValue> value,
        string path = "")
    {
        return this.innerPattern?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    /// <summary>
    /// Gets or sets a value pattern for a specific key.
    /// </summary>
    /// <param name="key">The dictionary key.</param>
    /// <returns>The pattern for the value at the specified key.</returns>
    /// <exception cref="NotImplementedException">This indexer is not currently implemented.</exception>
    public TDefaultValuePattern this[TKey key]
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Creates a dictionary pattern from a dictionary of values.
    /// </summary>
    /// <param name="value">The dictionary of values to match.</param>
    /// <returns>A dictionary pattern that matches all specified key-value pairs.</returns>
    public static IPattern<IDictionary<TKey, TValue>> From(
        IDictionary<TKey, TValue> value)
    {
        return new DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
            DictionaryPattern.Items(
                value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => TDefaultValuePattern.Create(kvp.Value))));
    }

    /// <summary>
    /// Implicitly converts a dictionary pattern to a dictionary pattern default.
    /// </summary>
    /// <param name="pattern">The dictionary pattern to convert.</param>
    public static implicit operator DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
        DictionaryPattern<TKey, TValue> pattern)
    {
        return new(pattern);
    }
}