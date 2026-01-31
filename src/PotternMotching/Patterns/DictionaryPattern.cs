namespace PotternMotching.Patterns;

using PotternMotching.Matchers;

/// <summary>
/// Represents a pattern for matching dictionaries.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <remarks>
/// This struct wraps a <see cref="DictionaryMatcher{TKey, TValue}"/> and tracks whether the pattern has been set.
/// It supports implicit conversion from dictionaries for convenient pattern creation.
/// Used in auto-generated pattern classes for Dictionary and IDictionary properties.
/// </remarks>
public readonly struct DictionaryPattern<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of <see cref="DictionaryPattern{TKey, TValue}"/>.
    /// </summary>
    /// <param name="matcher">The dictionary matcher to use.</param>
    public DictionaryPattern(
        DictionaryMatcher<TKey, TValue> matcher)
    {
        this.Matcher = matcher;
        this.IsSet = true;
    }

    /// <summary>
    /// Gets the underlying dictionary matcher.
    /// </summary>
    /// <remarks>
    /// This will be null if the pattern was not set (default value).
    /// In pattern matching, null values are treated as wildcards (always match).
    /// </remarks>
    public DictionaryMatcher<TKey, TValue> Matcher { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has been explicitly set.
    /// </summary>
    public bool IsSet { get; }

    /// <summary>
    /// Implicitly converts a dictionary to a pattern that matches all key-value pairs exactly.
    /// </summary>
    /// <param name="values">The dictionary of expected key-value pairs.</param>
    /// <returns>A dictionary pattern that matches all specified pairs.</returns>
    /// <example>
    /// <code>
    /// DictionaryPattern&lt;string, int&gt; pattern = new Dictionary&lt;string, int&gt;
    /// {
    ///     ["key1"] = 42,
    ///     ["key2"] = 100
    /// };
    /// </code>
    /// </example>
    public static implicit operator DictionaryPattern<TKey, TValue>(
        Dictionary<TKey, TValue> values)
    {
        var matchers = values.ToDictionary(
            kvp => kvp.Key,
            kvp => (IMatcher<TValue>)ValueMatcher.Exact(kvp.Value));

        return new(DictionaryMatcher.MatchAll(matchers));
    }
}