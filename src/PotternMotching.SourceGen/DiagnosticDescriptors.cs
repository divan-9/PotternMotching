namespace PotternMotching.SourceGen;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    private const string Category = "PotternMotching.SourceGen";

    public static readonly DiagnosticDescriptor TypeMustBeRecord = new(
        id: "PM0001",
        title: "Type must be a record",
        messageFormat: "Type '{0}' must be a record to use [AutoPattern]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InheritanceNotSupported = new(
        id: "PM0002",
        title: "Inheritance is not supported",
        messageFormat: "Type '{0}' cannot have a base type other than object when using [AutoPattern]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoPrimaryConstructor = new(
        id: "PM0003",
        title: "No primary constructor found",
        messageFormat: "Type '{0}' must have a primary constructor to use [AutoPattern]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestedTypePatternNotFound = new(
        id: "PM0004",
        title: "Nested type pattern not found",
        messageFormat: "Property '{0}' of type '{1}' may need [AutoPattern] attribute for automatic pattern generation",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMustBePartial = new(
        id: "PM0005",
        title: "Union type must be partial",
        messageFormat: "Union type '{0}' must be declared as partial to use [AutoPattern]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMustHaveVariants = new(
        id: "PM0006",
        title: "Union type must have variant types",
        messageFormat: "Union type '{0}' must have at least one nested record variant",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
