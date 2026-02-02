namespace PotternMotching.Matchers;

using System.Runtime.CompilerServices;
using Dunet;

[Union]
public partial record CollectionMatcher<T> : IMatcher<IEnumerable<T>>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        IEnumerable<T> value,
        string path = "")
    {
        return this.Match(
            anyElement => anyElement.EvaluateAnyElement(value, path),
            anyOrder => anyOrder.EvaluateAnyOrder(value, path),
            sequence => sequence.EvaluateSequence(value, path));
    }

    public partial record AnyElement(
        IMatcher<T> Pattern);

    public partial record Sequence(
        IMatcher<T>[] Patterns);

    public partial record AnyOrder(
        IMatcher<T>[] Patterns);

    public static implicit operator DefaultMatcher<IEnumerable<T>>(
        CollectionMatcher<T> matcher)
    {
        return new DefaultMatcher<IEnumerable<T>>(matcher);
    }
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
    public static CollectionMatcher<T>.Sequence AnyOrder<T>(
        IMatcher<T>[] example)
    {
        return new CollectionMatcher<T>.AnyOrder(example);
    }

    public static CollectionMatcher<T>.Sequence AnyOrder<T>(
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