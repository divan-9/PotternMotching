namespace PatternMatching.RuleBuilders;

using System.Collections;
using System.Runtime.CompilerServices;
using PatternMatching.Rules;

[CollectionBuilder(typeof(SetRuleBuilder), "Create")]
public readonly struct SetRuleBuilder<T> : IReadOnlyCollection<T>
{
    public SetRuleBuilder(
        CollectionRule<T>.MatchAll values)
    {
        this.Values = values;
        this.IsSet = true;
    }

    public CollectionRule<T>.MatchAll Values { get; }

    public bool IsSet { get; }

    public int Count => throw new NotImplementedException();

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public static implicit operator SetRuleBuilder<T>(
        HashSet<T> values)
    {
        return new(new CollectionRule<T>.MatchAll(values));
    }

    public static implicit operator SetRuleBuilder<T>(
        CollectionRule<T>.MatchAll rule)
    {
        return new(rule);
    }
}

internal static class SetRuleBuilder
{
    public static SetRuleBuilder<T> Create<T>(
        ReadOnlySpan<T> values)
    {
        return new SetRuleBuilder<T>(
            new CollectionRule<T>.MatchAll([.. values]));
    }
}