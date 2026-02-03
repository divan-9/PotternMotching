namespace PotternMotching.Matchers;

using System.Runtime.CompilerServices;
using Dunet;

[Union]
public partial record CollectionMatcher<T> : IMatcher<IEnumerable<T>>
{
    public static IMatcher<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return CollectionMatcher.Sequence(
            [.. value.Select(v => new ValueMatcher<T>.Exact(v))]);
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.Match(
            anyElement => anyElement.EvaluateAnyElement(value, path),
            sequence => sequence.EvaluateSequence(value, path),
            anyOrder => anyOrder.EvaluateAnyOrder(value, path));
    }

    public partial record AnyElement(
        IMatcher<T> Pattern);

    public partial record Sequence(
        IMatcher<T>[] Patterns);

    public partial record AnyOrder(
        IMatcher<T>[] Patterns);
}

public static class CollectionMatcher
{
    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.Sequence Sequence<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.Sequence(example);
    }

    public static CollectionMatcher<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.Sequence([.. example.Select(ValueMatcher.Exact)]);
    }

    [OverloadResolutionPriority(1)]
    public static CollectionMatcher<T>.AnyOrder AnyOrder<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.AnyOrder(example);
    }

    public static CollectionMatcher<T>.AnyOrder AnyOrder<T>(
        T[] example)
    {
        return new CollectionMatcher<T>.AnyOrder([.. example.Select(ValueMatcher.Exact)]);
    }

    public static CollectionMatcher<T>.AnyElement AnyElement<T>(
        IMatcher<T> pattern)
    {
        return new CollectionMatcher<T>.AnyElement(pattern);
    }

    public static CollectionMatcher<T>.AnyElement AnyElement<T>(
        T value)
    {
        return new CollectionMatcher<T>.AnyElement(ValueMatcher.Exact(value));
    }
}