namespace PatternMatching.Patterns;

using Dunet;

[Union]
public partial record DictionaryPattern<TKey, TValue> : IPattern
    where TKey : notnull
{
    public partial record MatchAll(
        Dictionary<TKey, TValue> items);
}

public static class DictionaryPattern
{
    public static DictionaryPattern<TKey, TValue>.MatchAll MatchAll<TKey, TValue>(
        Dictionary<TKey, TValue> patterns)
        where TKey : notnull
    {
        return new DictionaryPattern<TKey, TValue>.MatchAll(patterns);
    }
}