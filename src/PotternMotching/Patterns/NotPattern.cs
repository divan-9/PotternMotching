namespace PotternMotching.Patterns;

/// <summary>
/// Matches when the inner pattern does not match the value.
/// </summary>
/// <typeparam name="T">The type of value to match.</typeparam>
/// <param name="Inner">The pattern whose result is negated.</param>
public sealed record NotPattern<T>(IPattern<T> Inner) : IPattern<T>
{
    /// <inheritdoc />
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return Inner.Evaluate(value, path) is MatchResult.Failure
            ? new MatchResult.Success()
            : new MatchResult.Failure([
                $"{path}: [Pattern.Not] Expected {Inner} not to match, but it matched"
            ]);
    }
}

/// <summary>
/// Factory methods for composing patterns.
/// </summary>
public static class Pattern
{
    /// <summary>
    /// Creates a pattern that matches exactly when the inner pattern fails.
    /// </summary>
    /// <typeparam name="T">The type of value to match.</typeparam>
    /// <param name="inner">The pattern to negate.</param>
    /// <returns>A negated pattern.</returns>
    public static NotPattern<T> Not<T>(
        IPattern<T> inner)
    {
        return new NotPattern<T>(inner);
    }

    /// <summary>
    /// Negates a mutable dictionary pattern. Specify the read-only interface explicitly
    /// with <see cref="Not{T}(IPattern{T})"/> when matching a read-only dictionary.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="inner">The dictionary pattern to negate.</param>
    /// <returns>A negated dictionary pattern.</returns>
    public static NotPattern<IDictionary<TKey, TValue>> Not<TKey, TValue>(
        DictionaryPattern<TKey, TValue> inner)
        where TKey : notnull
    {
        return new NotPattern<IDictionary<TKey, TValue>>(inner);
    }
}
