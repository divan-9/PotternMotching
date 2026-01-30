namespace PatternMatching.RuleBuilders;

using PatternMatching.Rules;

public readonly struct ValueRuleBuilder<T>
{
    public ValueRuleBuilder(
        ValueRule<T>? value)
    {
        this.Value = value;
        this.IsSet = true;
    }

    public ValueRule<T>? Value { get; }
    public bool IsSet { get; }

    public static implicit operator ValueRuleBuilder<T>(
        T value)
    {
        return new(new ValueRule<T>.Exact(value));
    }
}