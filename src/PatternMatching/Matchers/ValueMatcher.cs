namespace PatternMatching.Matchers;

using Dunet;

[Union]
public partial record ValueMatcher<T> : IPattern
{
    public partial record Exact(
        T Value);
}

public static class ValueMatcher
{
    public static ValueMatcher<T>.Exact Exact<T>(
        T value)
    {
        return new ValueMatcher<T>.Exact(value);
    }
}