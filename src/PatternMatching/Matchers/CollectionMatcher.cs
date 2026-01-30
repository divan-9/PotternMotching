namespace PatternMatching.Matchers;

using System.Runtime.CompilerServices;
using Dunet;

[Union]
public partial record CollectionMatcher<T> : IMatcher<IEnumerable<T>>
{
    public MatchResult Match(
        IEnumerable<T> value,
        string path = "")
    {
        throw new NotImplementedException();
    }

    public partial record MatchAll(
        IMatcher<T>[] items);

    public partial record Sequence(
        IMatcher<T>[] items);

    public partial record EndsWith(
        IMatcher<T>[] items);

    public partial record StartsWith(
        IMatcher<T>[] items);
}

public static class CollectionMatcher
{
    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.MatchAll MatchAll<T>(
        IMatcher<T>[] patterns)
    {
        return new CollectionMatcher<T>.MatchAll(patterns);
    }

    public static CollectionMatcher<T>.MatchAll MatchAll<T>(
        T[] patterns)
    {
        return new CollectionMatcher<T>.MatchAll([.. patterns.Select(ValueMatcher.Exact)]);
    }

    public static CollectionMatcher<T>.Sequence Sequence<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.Sequence(example);
    }

    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.EndsWith EndsWith<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.EndsWith(example);
    }

    public static CollectionMatcher<T>.EndsWith EndsWith<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.EndsWith([.. example.Select(ValueMatcher.Exact)]);
    }

    public static CollectionMatcher<T>.StartsWith StartsWith<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.StartsWith(example);
    }
}