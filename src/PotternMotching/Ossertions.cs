namespace PotternMotching;

using System.Runtime.CompilerServices;
using PotternMotching.Patterns;

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
    /// person.AssertPattern(pattern); // Passes
    ///
    /// var wrongPerson = new Person("Bob", 25);
    /// wrongPerson.AssertPattern(pattern); // Throws AssertionFailedException with details
    /// </code>
    /// </example>
    public static void AssertPattern<T>(
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

    /// <summary>
    /// Asserts that the target value exactly matches the specified example value.
    /// </summary>
    /// <typeparam name="T">The type of the value to compare.</typeparam>
    /// <param name="target">The actual value being asserted.</param>
    /// <param name="example">The expected value to compare against.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target value does not exactly match the expected example value.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="ValuePattern.Exact{T}(T)"/>.
    /// It is useful when you want an exact-value assertion without explicitly constructing a pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// var person = new Person("Alice", 30);
    ///
    /// person.AssertExact(new Person("Alice", 30)); // Passes
    /// person.AssertExact(new Person("Bob", 25));   // Throws AssertionFailedException
    /// </code>
    /// </example>
    public static void AssertExact<T>(
        this T target,
        T example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(ValuePattern.Exact(example), path);
    }

    /// <summary>
    /// Asserts that the target collection contains all items from the specified example collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The collection of expected items that must all be present in the target.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not contain all expected items.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.Subset{T}(T[])"/>.
    /// It is useful when you want to assert subset membership without explicitly constructing a collection pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// var tags = new[] { "important", "backend", "urgent" };
    ///
    /// tags.AssertSubset(["important", "urgent"]); // Passes
    /// tags.AssertSubset(["frontend"]);             // Throws AssertionFailedException
    /// </code>
    /// </example>
    public static void AssertSubset<T>(
        this IEnumerable<T> target,
        IPattern<T>[] example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.Subset(example), path);
    }

    /// <summary>
    /// Asserts that the target collection contains all items from the specified example collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The collection of expected items that must all be present in the target.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not contain all expected items.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.Subset{T}(T[])"/>.
    /// It is useful when you want to assert subset membership without explicitly constructing a collection pattern.
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static void AssertSubset<T>(
        this IEnumerable<T> target,
        IEnumerable<T> example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.Subset([.. example]), path);
    }

    /// <summary>
    /// Asserts that the target collection exactly matches the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not exactly match the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.Sequence{T}(T[])"/>.
    /// It is useful when you want to assert an exact sequence without explicitly constructing a collection pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// var values = new[] { 1, 2, 3 };
    ///
    /// values.AssertSequence([1, 2, 3]); // Passes
    /// values.AssertSequence([1, 3]);    // Throws AssertionFailedException
    /// </code>
    /// </example>
    [OverloadResolutionPriority(1)]
    public static void AssertSequence<T>(
        this IEnumerable<T> target,
        IPattern<T>[] example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.Sequence(example), path);
    }

    /// <summary>
    /// Asserts that the target collection exactly matches the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not exactly match the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.Sequence{T}(T[])"/>.
    /// It is useful when you want to assert an exact sequence without explicitly constructing a collection pattern.
    /// </remarks>
    public static void AssertSequence<T>(
        this IEnumerable<T> target,
        IEnumerable<T> example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.Sequence([.. example]), path);
    }

    /// <summary>
    /// Asserts that the target collection ends with the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected ending sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not end with the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.EndsWith{T}(T[])"/>.
    /// It is useful when you want to assert a collection suffix without explicitly constructing a collection pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// var values = new[] { 1, 2, 3 };
    ///
    /// values.AssertEndsWith([2, 3]); // Passes
    /// values.AssertEndsWith([1, 2]); // Throws AssertionFailedException
    /// </code>
    /// </example>
    [OverloadResolutionPriority(1)]
    public static void AssertEndsWith<T>(
        this IEnumerable<T> target,
        IPattern<T>[] example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.EndsWith(example), path);
    }

    /// <summary>
    /// Asserts that the target collection ends with the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected ending sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not end with the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.EndsWith{T}(T[])"/>.
    /// It is useful when you want to assert a collection suffix without explicitly constructing a collection pattern.
    /// </remarks>
    public static void AssertEndsWith<T>(
        this IEnumerable<T> target,
        IEnumerable<T> example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.EndsWith([.. example]), path);
    }

    /// <summary>
    /// Asserts that the target collection starts with the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected starting sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not start with the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.StartsWith{T}(T[])"/>.
    /// It is useful when you want to assert a collection prefix without explicitly constructing a collection pattern.
    /// </remarks>
    /// <example>
    /// <code>
    /// var values = new[] { 1, 2, 3 };
    ///
    /// values.AssertStartsWith([1, 2]); // Passes
    /// values.AssertStartsWith([2, 3]); // Throws AssertionFailedException
    /// </code>
    /// </example>
    [OverloadResolutionPriority(1)]
    public static void AssertStartsWith<T>(
        this IEnumerable<T> target,
        IPattern<T>[] example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.StartsWith(example), path);
    }

    /// <summary>
    /// Asserts that the target collection starts with the specified example sequence.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <param name="target">The actual collection being asserted.</param>
    /// <param name="example">The expected starting sequence of items.</param>
    /// <param name="path">
    /// The expression path of the target value. This is automatically captured from the caller's code
    /// using <see cref="CallerArgumentExpressionAttribute"/> and is used in error messages.
    /// </param>
    /// <exception cref="AssertionFailedException">
    /// Thrown when the target collection does not start with the expected sequence.
    /// </exception>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="CollectionPattern.StartsWith{T}(T[])"/>.
    /// It is useful when you want to assert a collection prefix without explicitly constructing a collection pattern.
    /// </remarks>
    public static void AssertStartsWith<T>(
        this IEnumerable<T> target,
        IEnumerable<T> example,
        [CallerArgumentExpression(nameof(target))] string? path = null)
    {
        target.AssertPattern(CollectionPattern.StartsWith([.. example]), path);
    }
}