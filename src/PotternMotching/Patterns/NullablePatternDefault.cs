namespace PotternMotching.Patterns;

/// <summary>
/// Adapts a non-nullable object pattern so it can be evaluated against a nullable value.
/// </summary>
/// <typeparam name="T">The non-nullable reference type to match.</typeparam>
/// <param name="Inner">The pattern to evaluate when the actual value is not null.</param>
public sealed record NullablePattern<T>(
    IPattern<T> Inner) : IPattern<T?>
    where T : class
{
    /// <inheritdoc />
    public MatchResult Evaluate(
        T? value,
        string path = "")
    {
        if (value is null)
        {
            return new MatchResult.Failure([
                $"{path}: Actual value is null"
            ]);
        }

        return Inner.Evaluate(value, path);
    }
}

/// <summary>
/// A wrapper type for optional nullable nested pattern matching with implicit conversions.
/// </summary>
/// <typeparam name="T">The non-nullable reference type to match.</typeparam>
/// <typeparam name="TPatternDefault">The default non-null pattern implementation.</typeparam>
/// <remarks>
/// This type is used in generated pattern classes for nullable nested properties. When unspecified,
/// it matches any value. A nested pattern requires a non-null actual value, while null values can be
/// matched explicitly with <see cref="ValuePattern.Null"/> or an exact nullable value pattern.
/// </remarks>
public readonly struct NullablePatternDefault<T, TPatternDefault> : IPattern<T?>, IPatternConstructor<T?>
    where T : class
    where TPatternDefault : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<T?>? innerPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="NullablePatternDefault{T, TPatternDefault}"/> struct.
    /// </summary>
    /// <param name="pattern">The inner nullable pattern to use for matching.</param>
    public NullablePatternDefault(
        IPattern<T?> pattern)
    {
        this.innerPattern = pattern;
    }

    /// <summary>
    /// Creates a nullable pattern from a nullable value.
    /// </summary>
    /// <param name="value">The value to create a pattern from.</param>
    /// <returns>A pattern that matches the specified nullable value.</returns>
    public static IPattern<T?> From(
        T? value)
    {
        if (value is null)
        {
            return new NullablePatternDefault<T, TPatternDefault>(ValuePattern.Exact<T?>(null));
        }

        return new NullablePatternDefault<T, TPatternDefault>(
            new NullablePattern<T>(TPatternDefault.Create(value)));
    }

    /// <inheritdoc cref="From" />
    public static IPattern<T?> Create(
        T? value)
    {
        return From(value);
    }

    /// <inheritdoc />
    public MatchResult Evaluate(
        T? value,
        string path = "")
    {
        return this.innerPattern?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    /// <summary>
    /// Implicitly converts a nullable value to a nullable pattern default.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator NullablePatternDefault<T, TPatternDefault>(
        T? value)
    {
        return (NullablePatternDefault<T, TPatternDefault>)From(value);
    }

    /// <summary>
    /// Implicitly converts a non-null nested pattern to a nullable pattern default.
    /// </summary>
    /// <param name="pattern">The nested pattern to convert.</param>
    public static implicit operator NullablePatternDefault<T, TPatternDefault>(
        TPatternDefault pattern)
    {
        return new NullablePatternDefault<T, TPatternDefault>(new NullablePattern<T>(pattern));
    }

    /// <summary>
    /// Implicitly converts an exact nullable value pattern to a nullable pattern default.
    /// </summary>
    /// <param name="pattern">The exact nullable value pattern to convert.</param>
    public static implicit operator NullablePatternDefault<T, TPatternDefault>(
        ValuePattern<T?>.Exact pattern)
    {
        return new NullablePatternDefault<T, TPatternDefault>(pattern);
    }


    /// <summary>
    /// Implicitly converts a null marker to a nullable pattern default.
    /// </summary>
    /// <param name="value">The null marker to convert.</param>
    public static implicit operator NullablePatternDefault<T, TPatternDefault>(
        ValuePattern.NullPatternToken value)
    {
        return new NullablePatternDefault<T, TPatternDefault>(ValuePattern.Exact<T?>(null));
    }
}
