namespace PatternMatching.Patterns;

using PatternMatching.Matchers;

public readonly struct DictionaryPattern<TKey, TValue>
    where TKey : notnull
{
    public DictionaryPattern(
        DictionaryMatcher<TKey, TValue> values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public DictionaryMatcher<TKey, TValue> Values { get; }
    public bool IsSet { get; }

    public static implicit operator DictionaryPattern<TKey, TValue>(
        Dictionary<TKey, TValue> values)
    {
        var matchers = values.ToDictionary(
            kvp => kvp.Key,
            kvp => (IMatcher<TValue>)ValueMatcher.Exact(kvp.Value));

        return new(DictionaryMatcher.MatchAll(matchers));
    }
}