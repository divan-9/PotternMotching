namespace PotternMotching.Matchers;

using Dunet;

/// <summary>
/// Provides matchers for dictionaries.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <remarks>
/// This is a discriminated union type representing different ways to match dictionaries.
/// Currently supports matching all specified key-value pairs.
/// </remarks>
[Union]
public partial record DictionaryMatcher<TKey, TValue> : IMatcher<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        IDictionary<TKey, TValue> value,
        string path = "")
    {
        return this.Match(
            matchAll => matchAll.EvaluateMatchAll(value, path),
            exactKeys => exactKeys.EvaluateExactKeys(value, path));
    }

    /// <summary>
    /// Matches if all specified key-value pairs are present in the dictionary.
    /// </summary>
    /// <param name="items">The expected key-value pairs, where values are matchers.</param>
    /// <remarks>
    /// The dictionary may contain additional keys not specified in the pattern.
    /// Each specified key must be present and its value must match the corresponding matcher.
    /// </remarks>
    public partial record MatchAll(
        Dictionary<TKey, IMatcher<TValue>> items);

    /// <summary>
    /// Matches if the dictionary contains exactly the specified keys with matching values.
    /// </summary>
    /// <param name="items">The expected key-value pairs, where values are matchers.</param>
    /// <remarks>
    /// The dictionary must not contain any additional keys beyond those specified.
    /// All specified keys must be present and their values must match the corresponding matchers.
    /// </remarks>
    public partial record ExactKeys(
        Dictionary<TKey, IMatcher<TValue>> items);
}

/// <summary>
/// Provides factory methods for creating dictionary matchers.
/// </summary>
public static class DictionaryMatcher
{
    /// <summary>
    /// Creates a matcher that succeeds if all specified key-value pairs are present in the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="patterns">The expected key-value pairs, where values are matchers.</param>
    /// <returns>A MatchAll dictionary matcher.</returns>
    /// <remarks>
    /// The dictionary being matched may contain additional keys. Only the specified keys are validated.
    /// </remarks>
    public static DictionaryMatcher<TKey, TValue>.MatchAll MatchAll<TKey, TValue>(
        Dictionary<TKey, IMatcher<TValue>> patterns)
        where TKey : notnull
    {
        return new DictionaryMatcher<TKey, TValue>.MatchAll(patterns);
    }

    /// <summary>
    /// Creates a matcher that succeeds if the dictionary contains exactly the specified keys with matching values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="patterns">The expected key-value pairs, where values are matchers.</param>
    /// <returns>An ExactKeys dictionary matcher.</returns>
    /// <remarks>
    /// The dictionary must not contain any keys beyond those specified. All specified keys must be present.
    /// </remarks>
    /// <example>
    /// <code>
    /// var matcher = DictionaryMatcher.ExactKeys(new Dictionary&lt;string, IMatcher&lt;int&gt;&gt;
    /// {
    ///     ["key1"] = ValueMatcher.Exact(42)
    /// });
    /// matcher.Evaluate(new Dictionary&lt;string, int&gt; { ["key1"] = 42 }); // Success
    /// matcher.Evaluate(new Dictionary&lt;string, int&gt; { ["key1"] = 42, ["key2"] = 100 }); // Failure - extra key
    /// </code>
    /// </example>
    public static DictionaryMatcher<TKey, TValue>.ExactKeys ExactKeys<TKey, TValue>(
        Dictionary<TKey, IMatcher<TValue>> patterns)
        where TKey : notnull
    {
        return new DictionaryMatcher<TKey, TValue>.ExactKeys(patterns);
    }
}