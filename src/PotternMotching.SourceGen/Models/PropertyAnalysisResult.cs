namespace PotternMotching.SourceGen.Models;

using Microsoft.CodeAnalysis;

public sealed class PropertyAnalysisResult
{
    public PropertyAnalysisResult(
        string propertyName,
        string propertyType,
        PatternWrapperKind wrapperKind,
        string? elementType = null,
        string? keyType = null,
        string? valueType = null,
        bool requiresNestedPattern = false,
        INamedTypeSymbol? nestedType = null,
        string? nestedPatternType = null,
        ITypeSymbol? propertyTypeSymbol = null)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        WrapperKind = wrapperKind;
        ElementType = elementType;
        KeyType = keyType;
        ValueType = valueType;
        RequiresNestedPattern = requiresNestedPattern;
        NestedType = nestedType;
        NestedPatternType = nestedPatternType;
        PropertyTypeSymbol = propertyTypeSymbol;
    }

    public string PropertyName { get; }
    public string PropertyType { get; }
    public PatternWrapperKind WrapperKind { get; }
    public string? ElementType { get; }
    public string? KeyType { get; }
    public string? ValueType { get; }
    public bool RequiresNestedPattern { get; }
    public INamedTypeSymbol? NestedType { get; }
    public string? NestedPatternType { get; }
    public ITypeSymbol? PropertyTypeSymbol { get; }
}
