namespace PotternMotching;

/// <summary>
/// Marks a record type for automatic pattern class generation.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a record type, a source generator will automatically create a corresponding
/// pattern class named <c>{RecordName}Pattern</c> in the same namespace. The pattern class
/// will have properties matching the record's primary constructor parameters, using appropriate
/// pattern wrapper types.
/// </para>
/// <para>
/// <strong>Supported Types:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Records with primary constructors (no inheritance except from object)</description></item>
/// <item><description>Primitive types and value types (mapped to <see cref="Matchers.DefaultMatcher{T}"/>)</description></item>
/// <item><description>Arrays and lists (mapped to <see cref="Matchers.DefaultCollectionMatcher{T, TDefaultItemMatcher}"/>)</description></item>
/// <item><description>HashSet and ISet (mapped to <see cref="Matchers.DefaultCollectionMatcher{T, TDefaultItemMatcher}"/>)</description></item>
/// <item><description>Dictionary and IDictionary (mapped to <see cref="Matchers.DefaultMatcher{T}"/> where T is IDictionary)</description></item>
/// <item><description>Nested records with [AutoPattern] (mapped to their pattern types)</description></item>
/// </list>
/// <para>
/// <strong>Example:</strong>
/// </para>
/// <code>
/// [AutoPattern]
/// public record Person(string Name, int Age);
///
/// // Automatically generates:
/// // public sealed record PersonPattern(
/// //     DefaultMatcher&lt;string&gt; Name = default,
/// //     DefaultMatcher&lt;int&gt; Age = default
/// // ) : IMatcher&lt;Person&gt; { ... }
///
/// // Usage:
/// var pattern = new PersonPattern(Name: "Alice", Age: 30);
/// var person = new Person("Alice", 30);
/// pattern.Evaluate(person); // Success
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class AutoPatternAttribute : Attribute
{
}