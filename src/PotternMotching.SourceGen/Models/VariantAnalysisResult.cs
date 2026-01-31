using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace PotternMotching.SourceGen.Models;

public sealed class VariantAnalysisResult
{
    public VariantAnalysisResult(
        INamedTypeSymbol variantSymbol,
        ImmutableArray<PropertyAnalysisResult> properties)
    {
        VariantSymbol = variantSymbol;
        Properties = properties;
    }

    public INamedTypeSymbol VariantSymbol { get; }
    public ImmutableArray<PropertyAnalysisResult> Properties { get; }
    public string VariantName => VariantSymbol.Name;
}
