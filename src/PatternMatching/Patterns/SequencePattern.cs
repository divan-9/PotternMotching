namespace PatternMatching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Matchers;

[CollectionBuilder(typeof(SequencePattern), "Create")]
public readonly struct SequencePattern<T> : IReadOnlyCollection<T>
{
    public SequencePattern(
        CollectionMatcher<T> values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public CollectionMatcher<T> Values { get; }
    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public static implicit operator SequencePattern<T>(
        T[] values)
    {
        return new(new CollectionMatcher<T>.Sequence(values));
    }

    public static implicit operator SequencePattern<T>(
        CollectionMatcher<T> pattern)
    {
        return new(pattern);
    }
}

internal class SequencePattern
{
    public static SequencePattern<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return new SequencePattern<T>(
            new CollectionMatcher<T>.Sequence(
                values.ToArray()));
    }
}