namespace PatternMatching.PatternBuilders;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Patterns;

[CollectionBuilder(typeof(SequencePatternBuilder), "Create")]
public readonly struct SequencePatternBuilder<T> : IReadOnlyCollection<T>
{
    public SequencePatternBuilder(
        CollectionPattern<T> values)
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

    public CollectionPattern<T> Values { get; }
    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public static implicit operator SequencePatternBuilder<T>(
        T[] values)
    {
        return new(new CollectionPattern<T>.Sequence(values));
    }

    public static implicit operator SequencePatternBuilder<T>(
        CollectionPattern<T> pattern)
    {
        return new(pattern);
    }
}

internal class SequencePatternBuilder
{
    public static SequencePatternBuilder<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return new SequencePatternBuilder<T>(
            new CollectionPattern<T>.Sequence(
                values.ToArray()));
    }
}