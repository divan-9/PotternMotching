namespace PotternMotching.SourceGen;

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PotternMotching.SourceGen.Models;

[Generator]
public class AutoPatternGenerator : IIncrementalGenerator
{
    private const string AutoPatternForAttributeName = "PotternMotching.AutoPatternForAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create syntax provider to find candidate types
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static type => type is not null);

        // Combine with compilation
        var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

        // Generate source
        context.RegisterSourceOutput(compilationAndTypes,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        // Quick syntactic check: is it a type declaration with attributes?
        return node is TypeDeclarationSyntax typeDecl &&
               typeDecl.AttributeLists.Count > 0;
    }

    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        return HasRelevantAttribute(typeSymbol)
            ? typeDeclaration
            : null;
    }

    private static bool HasRelevantAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(IsRelevantAttribute);
    }

    private static bool IsRelevantAttribute(AttributeData attribute)
    {
        var fullName = attribute.AttributeClass?.ToDisplayString();
        return fullName == AutoPatternForAttributeName;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax> types,
        SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
            return;

        var processedSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var allRequests = ImmutableArray.CreateBuilder<GenerationRequest>();

        foreach (var typeDeclaration in types.Distinct())
        {
            var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
                continue;

            if (!processedSymbols.Add(typeSymbol))
                continue;

            var (requests, diagnostics) = GetGenerationRequests(typeSymbol);

            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            allRequests.AddRange(requests);
        }

        if (allRequests.Count == 0)
            return;

        var generationRequests = FilterCollisions(allRequests.ToImmutable(), context);
        var knownPatternTypes = BuildKnownPatternTypeMap(generationRequests);

        foreach (var request in generationRequests)
        {
            var analysis = TypeAnalyzer.AnalyzeTargetType(request.TargetSymbol, knownPatternTypes);

            foreach (var diagnostic in analysis.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            if (!analysis.IsValid)
                continue;

            var sourceCode = PatternCodeGenerator.Generate(
                analysis,
                request.GeneratedNamespace,
                request.GeneratedPatternName);

            context.AddSource(GetHintName(request), SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static (ImmutableArray<GenerationRequest> Requests, ImmutableArray<Diagnostic> Diagnostics) GetGenerationRequests(
        INamedTypeSymbol sourceSymbol)
    {
        var requests = ImmutableArray.CreateBuilder<GenerationRequest>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var attribute in sourceSymbol.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.ToDisplayString();

            if (attributeName != AutoPatternForAttributeName)
                continue;

            if (attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not ITypeSymbol targetType ||
                targetType is not INamedTypeSymbol targetSymbol)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.ExternalTargetCouldNotBeResolved,
                    attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? sourceSymbol.Locations.FirstOrDefault(),
                    sourceSymbol.Name));
                continue;
            }

            requests.Add(new GenerationRequest(
                sourceSymbol,
                targetSymbol,
                GetNamespace(sourceSymbol.ContainingNamespace),
                $"{targetSymbol.Name}Pattern"));
        }

        return (requests.ToImmutable(), diagnostics.ToImmutable());
    }

    private static ImmutableArray<GenerationRequest> FilterCollisions(
        ImmutableArray<GenerationRequest> requests,
        SourceProductionContext context)
    {
        var conflictingGeneratedTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var group in requests.GroupBy(r => GetGeneratedTypeFullName(r.GeneratedNamespace, r.GeneratedPatternName)))
        {
            if (group.Count() <= 1)
                continue;

            conflictingGeneratedTypes.Add(group.Key);

            foreach (var request in group)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.GeneratedPatternNameCollision,
                    request.SourceSymbol.Locations.FirstOrDefault(),
                    group.Key));
            }
        }

        return [..
            requests.Where(request =>
                !conflictingGeneratedTypes.Contains(GetGeneratedTypeFullName(request.GeneratedNamespace, request.GeneratedPatternName)))
        ];
    }

    private static ImmutableDictionary<INamedTypeSymbol, string> BuildKnownPatternTypeMap(
        ImmutableArray<GenerationRequest> requests)
    {
        var builder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);

        foreach (var request in requests)
        {
            if (!builder.ContainsKey(request.TargetSymbol))
            {
                builder.Add(request.TargetSymbol, GetGeneratedPatternTypeName(request.GeneratedNamespace, request.GeneratedPatternName));
            }
        }

        return builder.ToImmutable();
    }

    private static string GetNamespace(INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.IsGlobalNamespace
            ? string.Empty
            : namespaceSymbol.ToDisplayString();
    }

    private static string GetGeneratedTypeFullName(string generatedNamespace, string generatedPatternName)
    {
        return string.IsNullOrEmpty(generatedNamespace)
            ? generatedPatternName
            : $"{generatedNamespace}.{generatedPatternName}";
    }

    private static string GetGeneratedPatternTypeName(string generatedNamespace, string generatedPatternName)
    {
        return string.IsNullOrEmpty(generatedNamespace)
            ? generatedPatternName
            : $"global::{generatedNamespace}.{generatedPatternName}";
    }

    private static string GetHintName(GenerationRequest request)
    {
        return GetGeneratedTypeFullName(request.GeneratedNamespace, request.GeneratedPatternName)
            .Replace('.', '_') + ".g.cs";
    }
}
