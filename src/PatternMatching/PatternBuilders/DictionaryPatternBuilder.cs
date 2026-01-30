namespace PatternMatching.PatternBuilders;

using PatternMatching.Patterns;

public readonly struct DictionaryPatternBuilder<TKey, TValue>
    where TKey : notnull
{
    public DictionaryPatternBuilder(
        DictionaryPattern<TKey, TValue> values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public DictionaryPattern<TKey, TValue> Values { get; }
    public bool IsSet { get; }

    public static implicit operator DictionaryPatternBuilder<TKey, TValue>(
        Dictionary<TKey, TValue> values)
    {
        return new(values);
    }
}