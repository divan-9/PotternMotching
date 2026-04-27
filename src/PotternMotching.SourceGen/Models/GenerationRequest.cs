namespace PotternMotching.SourceGen.Models;

using Microsoft.CodeAnalysis;

internal sealed class GenerationRequest
{
    public GenerationRequest(
        INamedTypeSymbol sourceSymbol,
        INamedTypeSymbol targetSymbol,
        string generatedNamespace,
        string generatedPatternName,
        bool isOwnedType)
    {
        SourceSymbol = sourceSymbol;
        TargetSymbol = targetSymbol;
        GeneratedNamespace = generatedNamespace;
        GeneratedPatternName = generatedPatternName;
        IsOwnedType = isOwnedType;
    }

    public INamedTypeSymbol SourceSymbol { get; }
    public INamedTypeSymbol TargetSymbol { get; }
    public string GeneratedNamespace { get; }
    public string GeneratedPatternName { get; }
    public bool IsOwnedType { get; }
}
