namespace PatternMatching.Rules;

using Dunet;

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