namespace PotternMotching.SourceGen.Models;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

public sealed class TypeAnalysisResult
{
    public TypeAnalysisResult(
        INamedTypeSymbol typeSymbol,
        bool isValid,
        ImmutableArray<PropertyAnalysisResult> properties,
        ImmutableArray<Diagnostic> diagnostics,
        bool isUnion = false,
        ImmutableArray<VariantAnalysisResult> variants = default)
    {
        TypeSymbol = typeSymbol;
        IsValid = isValid;
        Properties = properties;
        Diagnostics = diagnostics;
        IsUnion = isUnion;
        Variants = variants.IsDefault ? ImmutableArray<VariantAnalysisResult>.Empty : variants;
    }

    public INamedTypeSymbol TypeSymbol { get; }
    public bool IsValid { get; }
    public ImmutableArray<PropertyAnalysisResult> Properties { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public bool IsUnion { get; }
    public ImmutableArray<VariantAnalysisResult> Variants { get; }
}
