namespace PatternMatching.Matchers;

using Dunet;

[Union]
public partial record CollectionMatcher<T> : IPattern
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

public static class CollectionMatcher
{
    public static CollectionMatcher<T>.MatchAll MatchAll<T>(
        HashSet<T> patterns)
    {
        return new CollectionMatcher<T>.MatchAll(patterns);
    }

    public static CollectionMatcher<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.Sequence(example);
    }

    public static CollectionMatcher<T>.EndsWith EndsWith<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.EndsWith(example);
    }

    public static CollectionMatcher<T>.StartsWith StartsWith<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.StartsWith(example);
    }
}