namespace PotternMotching.Patterns;

public readonly struct DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>
    : IPattern<IDictionary<TKey, TValue>>
    where TKey : notnull
    where TDefaultValuePattern : IPattern<TValue>, IPatternConstructor<TValue>
{
    private readonly IPattern<IDictionary<TKey, TValue>> innerPattern;

    public DictionaryPatternDefault()
    {
        this.innerPattern = new EmptyPattern<IDictionary<TKey, TValue>>();
    }

    public DictionaryPatternDefault(
        IPattern<IDictionary<TKey, TValue>> pattern)
    {
        this.innerPattern = pattern;
    }

    public MatchResult Evaluate(
        IDictionary<TKey, TValue> value,
        string path = "")
    {
        return this.innerPattern.Evaluate(value, path);
    }

    public TDefaultValuePattern this[TKey key]
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public static IPattern<IDictionary<TKey, TValue>> From(
        IDictionary<TKey, TValue> value)
    {
        return new DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
            DictionaryPattern.Items(
                value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => TDefaultValuePattern.Create(kvp.Value))));
    }

    public static implicit operator DictionaryPatternDefault<TKey, TValue, TDefaultValuePattern>(
        DictionaryPattern<TKey, TValue> pattern)
    {
        return new(pattern);
    }
}