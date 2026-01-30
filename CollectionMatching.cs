namespace PatternMatching;

using Dunet;

[Union]
public partial record CollectionRule<T> : IRule
{
    public partial record MatchAll(
        HashSet<T> items);

    public partial record Sequence(
        T[] items);

    public partial record EndsWith(
        T[] items);

    public partial record StartsWith(
        T[] items);
}

public static class CollectionRule
{
    public static CollectionRule<T>.MatchAll MatchAll<T>(
        HashSet<T> patterns)
    {
        return new CollectionRule<T>.MatchAll(patterns);
    }

    public static CollectionRule<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionRule<T>.Sequence(example);
    }

    public static CollectionRule<T>.EndsWith EndsWith<T>(
        T[] example)
    {
        return new CollectionRule<T>.EndsWith(example);
    }

    public static CollectionRule<T>.StartsWith StartsWith<T>(
        T[] example)
    {
        return new CollectionRule<T>.StartsWith(example);
    }
}