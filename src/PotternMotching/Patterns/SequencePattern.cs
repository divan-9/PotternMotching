namespace PotternMotching.Patterns;

using System.Collections;
using System.Runtime.CompilerServices;
using PotternMotching.Matchers;

[CollectionBuilder(typeof(SequencePattern), "Create")]
public readonly struct SequencePattern<TItem, TPattern> : IReadOnlyCollection<TPattern>
    where TPattern : IMatcher<TItem>
{
    public SequencePattern(
        CollectionMatcher<TItem> values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public CollectionMatcher<TItem> Values { get; }
    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public IEnumerator<TPattern> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    public static implicit operator SequencePattern<TItem, TPattern>(
        TPattern[] matchers)
    {
        return new(new CollectionMatcher<TItem>.Sequence([.. matchers]));
    }

    public static implicit operator SequencePattern<TItem, TPattern>(
        CollectionMatcher<TItem> pattern)
    {
        return new(pattern);
    }
}

public class SequencePattern
{
    public static SequencePattern<TItem, TPattern> Create<TItem, TPattern>(
        ReadOnlySpan<TPattern> values)
        where TPattern : IMatcher<TItem>
    {
        return values.ToArray();
    }
}