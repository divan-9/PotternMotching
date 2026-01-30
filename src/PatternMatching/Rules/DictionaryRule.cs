namespace PatternMatching.Rules;

using Dunet;

[Union]
public partial record DictionaryRule<TKey, TValue> : IRule
    where TKey : notnull
{
    public partial record MatchAll(
        Dictionary<TKey, TValue> items);
}

public static class DictionaryRule
{
    public static DictionaryRule<TKey, TValue>.MatchAll MatchAll<TKey, TValue>(
        Dictionary<TKey, TValue> patterns)
        where TKey : notnull
    {
        return new DictionaryRule<TKey, TValue>.MatchAll(patterns);
    }
}