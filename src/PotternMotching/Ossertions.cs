namespace PotternMotching;

using System.Runtime.CompilerServices;

/// <summary>
/// Provides assertion methods for pattern matching in tests.
/// </summary>
/// <remarks>
/// <para>
/// This class provides extension methods to assert that a value matches a specified pattern.
/// When the pattern does not match, an <see cref="AssertionFailedException"/> is thrown with
/// detailed information about the mismatch.
/// </para>
/// <para>
/// The method automatically captures the expression being tested using <see cref="CallerArgumentExpressionAttribute"/>
/// for better error messages.
/// </para>
/// </remarks>
public static class Ossertions
{
    /// <summary>
    /// Asserts that the target value matches the specified pattern.
    /// </summary>
    /// <typeparam name="T">The type of the value to match.</typeparam>
    /// <param name="target">The value to evaluate against the pattern.</param>
    /// <param name="pattern">The pattern that the value must match.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target value does not match the pattern. The exception message contains
    /// detailed information about all the reasons why the match failed.
    /// </exception>
    /// <example>
    /// <code>
    /// var person = new Person("Alice", 30);
    /// var pattern = new PersonPattern(Name: "Alice", Age: 30);
    ///
    /// person.Assert(pattern); // Passes
    ///
    /// var wrongPerson = new Person("Bob", 25);
    /// wrongPerson.Assert(pattern); // Throws AssertionFailedException with details
    /// </code>
    /// </example>
    public static void Assert<T>(
        this T target,
        IPattern<T> pattern,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        var result = pattern.Evaluate(
            value: target,
            path: path ?? string.Empty);

        result.Match(
            success: _ => { },
            failure: failure =>
            {
                var message = string.Join("\n", failure.Reasons.Select(x => $"\t\tFAILURE: {x}"));
                throw new AssertionFailedException($"\n{message}");
            });
    }
}
