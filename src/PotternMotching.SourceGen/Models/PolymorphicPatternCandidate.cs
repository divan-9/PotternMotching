namespace PotternMotching.SourceGen.Models;

using Microsoft.CodeAnalysis;

public sealed class PolymorphicPatternCandidate
{
    public PolymorphicPatternCandidate(
        INamedTypeSymbol typeSymbol,
        string patternTypeName)
    {
        TypeSymbol = typeSymbol;
        PatternTypeName = patternTypeName;
    }

    public INamedTypeSymbol TypeSymbol { get; }
    public string PatternTypeName { get; }
}
