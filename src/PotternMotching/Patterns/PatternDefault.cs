namespace PotternMotching.Patterns;

/// <summary>
/// A wrapper type that provides optional pattern matching with implicit conversions.
/// </summary>
/// <typeparam name="T">The type of value to match.</typeparam>
/// <typeparam name="TPatternDefault">The default pattern implementation to use for matching.</typeparam>
/// <remarks>
/// <para>
/// This type is used in generated pattern classes to provide flexible pattern matching with:
/// </para>
/// <list type="bullet">
/// <item><description>Default behavior - When not specified, the pattern is always considered matched (returns Success).</description></item>
/// <item><description>Implicit conversions from values - A value of type T is automatically wrapped in a pattern.</description></item>
/// <item><description>Implicit conversions from patterns - A pattern can be used directly.</description></item>
/// </list>
/// <para>
/// This enables concise syntax in generated pattern classes where properties can be left unspecified
/// (matching any value) or set to specific values or custom patterns.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a generated pattern class:
/// var pattern = new PersonPattern(
///     Name: "Alice",  // Implicitly converted from string
///     Age: default    // Will match any age
/// );
/// </code>
/// </example>
public readonly struct PatternDefault<T, TPatternDefault> : IPattern<T>, IPatternConstructor<T>
    where TPatternDefault : IPattern<T>, IPatternConstructor<T>
{
    private readonly IPattern<T>? innerMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="PatternDefault{T, TPatternDefault}"/> struct.
    /// </summary>
    /// <param name="matcher">The inner pattern to use for matching.</param>
    public PatternDefault(IPattern<T> matcher)
    {
        this.innerMatcher = matcher;
    }

    /// <summary>
    /// Creates a pattern from a value using the default pattern constructor.
    /// </summary>
    /// <param name="value">The value to create a pattern from.</param>
    /// <returns>A pattern that matches the specified value.</returns>
    public static IPattern<T> From(
        T value)
    {
        return new PatternDefault<T, TPatternDefault>(TPatternDefault.Create(value));
    }

    /// <inheritdoc cref="From"/>
    public static IPattern<T> Create(
        T value)
    {
        return From(value);
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(T value, string path = "")
    {
        return this.innerMatcher?.Evaluate(value, path) ?? new MatchResult.Success();
    }

    /// <summary>
    /// Implicitly converts a value to a pattern default.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator PatternDefault<T, TPatternDefault>(
        T value)
    {
        return new PatternDefault<T, TPatternDefault>(TPatternDefault.Create(value));
    }

    /// <summary>
    /// Implicitly converts a pattern to a pattern default.
    /// </summary>
    /// <param name="value">The pattern to convert.</param>
    public static implicit operator PatternDefault<T, TPatternDefault>(
        TPatternDefault value)
    {
        return new PatternDefault<T, TPatternDefault>(value);
    }
}