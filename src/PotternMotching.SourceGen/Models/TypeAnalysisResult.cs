namespace PotternMotching.SourceGen.Models;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

public sealed class TypeAnalysisResult
{
    public TypeAnalysisResult(
        INamedTypeSymbol typeSymbol,
        bool isValid,
        ImmutableArray<PropertyAnalysisResult> properties,
        ImmutableArray<Diagnostic> diagnostics)
    {
        TypeSymbol = typeSymbol;
        IsValid = isValid;
        Properties = properties;
        Diagnostics = diagnostics;
    }

    public INamedTypeSymbol TypeSymbol { get; }
    public bool IsValid { get; }
    public ImmutableArray<PropertyAnalysisResult> Properties { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
