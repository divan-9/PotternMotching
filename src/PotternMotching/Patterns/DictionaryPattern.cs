namespace PotternMotching.Patterns;

using Dunet;

/// <summary>
/// Pattern for matching dictionaries.
/// </summary>
[Union]
public partial record DictionaryPattern<TKey, TValue> : IPattern<IDictionary<TKey, TValue>>
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

    public static IPattern<IDictionary<TKey, TValue>> From(
        IDictionary<TKey, TValue> value)
    {
        return new DictionaryPattern<TKey, TValue>.Items(
            value.ToDictionary(
                kvp => kvp.Key,
                kvp => (IPattern<TValue>)new ValuePattern<TValue>.Exact(kvp.Value)));
    }

    public MatchResult Evaluate(
        IDictionary<TKey, TValue> value,
        string path = "")
    {
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
    public static DictionaryPattern<TKey, TValue>.Items Items<TKey, TValue>(
        Dictionary<TKey, IPattern<TValue>> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.Items(items);
    }

    public static DictionaryPattern<TKey, TValue>.Items Items<TKey, TValue>(
        Dictionary<TKey, TValue> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.Items(
            items.ToDictionary(
                kvp => kvp.Key,
                kvp => (IPattern<TValue>)new ValuePattern<TValue>.Exact(kvp.Value)));
    }

    public static DictionaryPattern<TKey, TValue>.ExactItems ExactItems<TKey, TValue>(
        Dictionary<TKey, IPattern<TValue>> items)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.ExactItems(items);
    }

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
