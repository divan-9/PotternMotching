namespace PatternMatching.Matchers;

using Dunet;

[Union]
public partial record DictionaryMatcher<TKey, TValue> : IMatcher<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    public MatchResult Match(
        IDictionary<TKey, TValue> value)
    {
        throw new NotImplementedException();
    }

    public partial record MatchAll(
        Dictionary<TKey, IMatcher<TValue>> items);
}

public static class DictionaryMatcher
{
    public static DictionaryMatcher<TKey, TValue>.MatchAll MatchAll<TKey, TValue>(
        Dictionary<TKey, IMatcher<TValue>> patterns)
        where TKey : notnull
    {
        return new DictionaryMatcher<TKey, TValue>.MatchAll(patterns);
    }
}