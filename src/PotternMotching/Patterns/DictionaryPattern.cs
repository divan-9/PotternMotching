namespace PotternMotching.Patterns;

using Dunet;

/// <summary>
/// Pattern for matching dictionaries.
/// </summary>
[Union]
public partial record DictionaryPattern<TKey, TValue> :
    IPattern<IDictionary<TKey, TValue>>,
    IPattern<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>
    /// Matches if all specified keys are present with matching values.
    /// Allows extra keys not in the pattern.
    /// </summary>
    public partial record Items(Dictionary<TKey, IPattern<TValue>> RequiredItems);

    /// <summary>
    /// Matches if the dictionary has exactly the specified keys (no more, no less)
    /// with matching values.
    /// </summary>
    public partial record ExactItems(Dictionary<TKey, IPattern<TValue>> RequiredItems);

    /// <summary>
    /// Creates a dictionary items pattern from a dictionary of expected values.
    /// </summary>
    /// <param name="value">The expected key-value pairs.</param>
    /// <returns>A dictionary pattern that matches all specified key-value pairs and allows extra keys.</returns>
    public static IPattern<IDictionary<TKey, TValue>> From(
        IDictionary<TKey, TValue> value)
    {
        return new DictionaryPattern<TKey, TValue>.Items(
            value.ToDictionary(
                kvp => kvp.Key,
                kvp => (IPattern<TValue>)new ValuePattern<TValue>.Exact(kvp.Value)));
    }

    /// <inheritdoc />
    public MatchResult Evaluate(
        IDictionary<TKey, TValue> value,
        string path = "")
    {
        if (value is null)
        {
            return new MatchResult.Failure([
                $"{path}: [DictionaryPattern] Actual dictionary is null"
            ]);
        }

        return this.Match(
            items => items.EvaluateItems(value, path),
            exactItems => exactItems.EvaluateExactItems(value, path));
    }

    MatchResult IPattern<IReadOnlyDictionary<TKey, TValue>>.Evaluate(
        IReadOnlyDictionary<TKey, TValue> value,
        string path)
    {
        if (value is null)
        {
            return new MatchResult.Failure([
                $"{path}: [DictionaryPattern] Actual dictionary is null"
            ]);
        }

        return this.Match(
            items => items.EvaluateItems(value, path),
            exactItems => exactItems.EvaluateExactItems(value, path));
    }
}

/// <summary>
/// Factory methods for creating dictionary patterns.
/// </summary>
public static class DictionaryPattern
{
    /// <summary>
    /// Creates a pattern that requires the specified key-pattern pairs and allows extra keys.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="items">The required key-pattern pairs.</param>
    /// <returns>A dictionary items pattern.</returns>
    public static DictionaryPattern<TKey, TValue>.Items Items<TKey, TValue>(
        Dictionary<TKey, IPattern<TValue>> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.Items(items);
    }

    /// <summary>
    /// Creates a pattern that requires the specified key-value pairs and allows extra keys.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="items">The required key-value pairs.</param>
    /// <returns>A dictionary items pattern with exact value patterns.</returns>
    public static DictionaryPattern<TKey, TValue>.Items Items<TKey, TValue>(
        Dictionary<TKey, TValue> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.Items(
            items.ToDictionary(
                kvp => kvp.Key,
                kvp => (IPattern<TValue>)new ValuePattern<TValue>.Exact(kvp.Value)));
    }

    /// <summary>
    /// Creates a pattern that requires exactly the specified key-pattern pairs.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="items">The exact key-pattern pairs.</param>
    /// <returns>A dictionary exact-items pattern.</returns>
    public static DictionaryPattern<TKey, TValue>.ExactItems ExactItems<TKey, TValue>(
        Dictionary<TKey, IPattern<TValue>> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.ExactItems(items);
    }

    /// <summary>
    /// Creates a pattern that requires exactly the specified key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="items">The exact key-value pairs.</param>
    /// <returns>A dictionary exact-items pattern with exact value patterns.</returns>
    public static DictionaryPattern<TKey, TValue>.ExactItems ExactItems<TKey, TValue>(
        Dictionary<TKey, TValue> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.ExactItems(
            items.ToDictionary(
                kvp => kvp.Key,
                kvp => (IPattern<TValue>)new ValuePattern<TValue>.Exact(kvp.Value)));
    }
}
