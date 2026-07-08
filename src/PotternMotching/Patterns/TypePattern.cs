namespace PotternMotching.Patterns;

/// <summary>
/// Adapts a pattern for a derived runtime type so it can be used where a pattern for the base type is required.
/// </summary>
/// <typeparam name="TBase">The base or interface type.</typeparam>
/// <typeparam name="TDerived">The concrete runtime type matched by the inner pattern.</typeparam>
public sealed class TypePattern<TBase, TDerived> : IPattern<TBase>
    where TDerived : TBase
{
    private readonly IPattern<TDerived> innerPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypePattern{TBase, TDerived}"/> class.
    /// </summary>
    /// <param name="innerPattern">The concrete pattern to adapt.</param>
    public TypePattern(IPattern<TDerived> innerPattern)
    {
        this.innerPattern = innerPattern;
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(TBase value, string path = "")
    {
        if (value is not TDerived typedValue)
        {
            var actualType = value?.GetType().Name ?? "null";
            return new MatchResult.Failure([
                $"{path}: Expected type {typeof(TDerived).Name} but got {actualType}"
            ]);
        }

        return this.innerPattern.Evaluate(typedValue, path);
    }
}
