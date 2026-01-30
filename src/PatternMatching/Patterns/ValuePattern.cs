namespace PatternMatching.Patterns;

using Dunet;

[Union]
public partial record ValuePattern<T> : IPattern
{
    public partial record Exact(
        T Value);
}

public static class ValuePattern
{
    public static ValuePattern<T>.Exact Exact<T>(
        T value)
    {
        return new ValuePattern<T>.Exact(value);
    }
}