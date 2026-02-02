namespace PotternMotching.SourceGen;

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class AutoPatternGenerator : IIncrementalGenerator
{
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

        // Check if it has the AutoPattern attribute using semantic model
        foreach (var attributeList in typeDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeClass = attributeSymbol.ContainingType;
                var fullName = attributeClass.ToDisplayString();

                if (fullName == "PotternMotching.AutoPatternAttribute")
                {
                    return typeDeclaration;
                }
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax> types,
        SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
            return;

        foreach (var typeDeclaration in types.Distinct())
        {
            // Get the semantic model for this syntax tree
            var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);

            // Get the type symbol
            if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
                continue;

            // Analyze the type
            var analysis = TypeAnalyzer.Analyze(typeSymbol, compilation);

            // Report diagnostics
            foreach (var diagnostic in analysis.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            // Generate code if valid
            if (analysis.IsValid)
            {
                var sourceCode = PatternCodeGenerator.Generate(analysis);
                var fileName = $"{typeSymbol.Name}Pattern.g.cs";

                context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
            }
        }
    }
}
