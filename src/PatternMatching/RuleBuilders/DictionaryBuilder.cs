namespace PatternMatching.RuleBuilders;

using PatternMatching.Rules;

public readonly struct DictionaryBuilder<TKey, TValue>
    where TKey : notnull
{
    public DictionaryBuilder(
        DictionaryRule<TKey, TValue> values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public DictionaryRule<TKey, TValue> Values { get; }
    public bool IsSet { get; }

    public static implicit operator DictionaryBuilder<TKey, TValue>(
        Dictionary<TKey, TValue> values)
    {
        return new(values);
    }
}