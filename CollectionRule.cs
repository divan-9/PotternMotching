namespace PatternMatching;

using Dunet;

public interface IRule;

[Union]
public partial record DictionaryRule<TKey, TValue> : IRule
    where TKey : notnull
{
    public partial record MatchAll(
        Dictionary<TKey, TValue> items);
}

[Union]
public partial record ValueRule<T> : IRule
{
    public partial record Exact(
        T Value);
}

public static class ValueRule
{
    public static ValueRule<T>.Exact Exact<T>(
        T value)
    {
        return new ValueRule<T>.Exact(value);
    }
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