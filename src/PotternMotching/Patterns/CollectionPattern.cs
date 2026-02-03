namespace PotternMotching.Patterns;

using System.Runtime.CompilerServices;
using Dunet;

[Union]
public partial record CollectionPattern<T> : IPattern<IEnumerable<T>>
{
    public static IPattern<IEnumerable<T>> From(
        IEnumerable<T> value)
    {
        return CollectionPattern.Sequence(
            [.. value.Select(v => new ValuePattern<T>.Exact(v))]);
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
        IPattern<T> Pattern);

    public partial record Sequence(
        IPattern<T>[] Patterns);

    public partial record AnyOrder(
        IPattern<T>[] Patterns);
}

public static class CollectionPattern
{
    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.Sequence Sequence<T>(
        IPattern<T>[] example)
    {
        return new CollectionPattern<T>.Sequence(example);
    }

    public static CollectionPattern<T>.Sequence Sequence<T>(
        T[] example)
    {
        return new CollectionPattern<T>.Sequence([.. example.Select(ValuePattern.Exact)]);
    }

    [OverloadResolutionPriority(1)]
    public static CollectionPattern<T>.AnyOrder AnyOrder<T>(
        IPattern<T>[] example)
    {
        return new CollectionPattern<T>.AnyOrder(example);
    }

    public static CollectionPattern<T>.AnyOrder AnyOrder<T>(
        T[] example)
    {
        return new CollectionPattern<T>.AnyOrder([.. example.Select(ValuePattern.Exact)]);
    }

    public static CollectionPattern<T>.AnyElement AnyElement<T>(
        IPattern<T> pattern)
    {
        return new CollectionPattern<T>.AnyElement(pattern);
    }

    public static CollectionPattern<T>.AnyElement AnyElement<T>(
        T value)
    {
        return new CollectionPattern<T>.AnyElement(ValuePattern.Exact(value));
    }
}