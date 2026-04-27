namespace PotternMotching;

/// <summary>
/// Marks an external record type for automatic pattern generation without modifying the target type itself.
/// </summary>
/// <remarks>
/// Apply this attribute to a local marker class to generate a pattern for the specified target record.
/// The generated pattern type is emitted into the marker class namespace.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoPatternForAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoPatternForAttribute"/> class.
    /// </summary>
    /// <param name="targetType">The external record type to generate a pattern for.</param>
    public AutoPatternForAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the external record type to generate a pattern for.
    /// </summary>
    public Type TargetType { get; }
}
