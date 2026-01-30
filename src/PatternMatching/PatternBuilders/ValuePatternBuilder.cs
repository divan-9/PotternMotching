namespace PatternMatching.PatternBuilders;

using PatternMatching.Patterns;

public readonly struct ValuePatternBuilder<T>
{
    public ValuePatternBuilder(
        ValuePattern<T>? value)
    {
        this.Value = value;
        this.IsSet = true;
    }

    public ValuePattern<T>? Value { get; }
    public bool IsSet { get; }

    public static implicit operator ValuePatternBuilder<T>(
        T value)
    {
        return new(new ValuePattern<T>.Exact(value));
    }
}