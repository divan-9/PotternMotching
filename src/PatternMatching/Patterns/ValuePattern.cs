namespace PatternMatching.Patterns;

using PatternMatching.Matchers;

public readonly struct ValuePattern<T>
{
    public ValuePattern(
        ValueMatcher<T>? value)
    {
        this.Value = value;
        this.IsSet = true;
    }

    public ValueMatcher<T>? Value { get; }
    public bool IsSet { get; }

    public static implicit operator ValuePattern<T>(
        T value)
    {
        return new(new ValueMatcher<T>.Exact(value));
    }
}