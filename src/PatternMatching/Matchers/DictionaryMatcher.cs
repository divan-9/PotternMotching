namespace PatternMatching.Matchers;

using Dunet;

[Union]
public partial record DictionaryMatcher<TKey, TValue> : IPattern
    where TKey : notnull
{
    public partial record MatchAll(
        Dictionary<TKey, TValue> items);
}

public static class DictionaryMatcher
{
    public static DictionaryMatcher<TKey, TValue>.MatchAll MatchAll<TKey, TValue>(
        Dictionary<TKey, TValue> patterns)
        where TKey : notnull
    {
        return new DictionaryMatcher<TKey, TValue>.MatchAll(patterns);
    }
}