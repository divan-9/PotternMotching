namespace PatternMatching.RuleBuilders;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Rules;

[CollectionBuilder(typeof(SequenceBuilder), "Create")]
public readonly struct SequenceBuilder<T> : IReadOnlyCollection<T>
{
    public SequenceBuilder(
        CollectionRule<T> values)
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

    public CollectionRule<T> Values { get; }
    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public static implicit operator SequenceBuilder<T>(
        T[] values)
    {
        return new(new CollectionRule<T>.Sequence(values));
    }

    public static implicit operator SequenceBuilder<T>(
        CollectionRule<T> rule)
    {
        return new(rule);
    }
}

internal class SequenceBuilder
{
    public static SequenceBuilder<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return new SequenceBuilder<T>(
            new CollectionRule<T>.Sequence(
                values.ToArray()));
    }
}