namespace PatternMatching.Patterns;

using Dunet;

[Union]
public partial record CollectionPattern<T> : IPattern
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

public static class CollectionPattern
{
    public static CollectionPattern<T>.MatchAll MatchAll<T>(
        HashSet<T> patterns)
    {
        return new CollectionPattern<T>.MatchAll(patterns);
    }

    public static CollectionPattern<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionPattern<T>.Sequence(example);
    }

    public static CollectionPattern<T>.EndsWith EndsWith<T>(
        T[] example)
    {
        return new CollectionPattern<T>.EndsWith(example);
    }

    public static CollectionPattern<T>.StartsWith StartsWith<T>(
        T[] example)
    {
        return new CollectionPattern<T>.StartsWith(example);
    }
}