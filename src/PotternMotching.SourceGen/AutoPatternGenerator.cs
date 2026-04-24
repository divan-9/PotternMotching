#pragma warning disable
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
        var byAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetTargetsByAttribute(ctx))
            .Where(static type => type is not null);

        var byAssert = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsAssertInvocation(node),
                transform: static (ctx, _) => GetTargetsByAssert(ctx))
            .Where(static symbol => symbol is not null);

        var compilationAndSymbols = context.CompilationProvider
            .Combine(byAttribute.Collect())
            .Combine(byAssert.Collect());

        context.RegisterSourceOutput(compilationAndSymbols, (spc, source) =>
        {
            var compilation = source.Left.Left;
            var attributedSymbols = source.Left.Right;
            var assertSymbols = source.Right;

            var all = attributedSymbols
                .Concat(assertSymbols)
                .Where(s => s is not null)
                .Distinct(SymbolEqualityComparer.Default)
                .Cast<INamedTypeSymbol>()
                .ToImmutableArray();

            Execute(compilation, all, spc);
        });

    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        // Quick syntactic check: is it a type declaration with attributes?
        return node is TypeDeclarationSyntax typeDecl &&
               typeDecl.AttributeLists.Count > 0;
    }

    private static bool IsAssertInvocation(SyntaxNode node)
    {
        return node is InvocationExpressionSyntax invocation &&
               invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Name.Identifier.Text == "Assert";
    }

    private static INamedTypeSymbol? GetTargetsByAssert(
        GeneratorSyntaxContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Name.Identifier.Text != "Assert") 
        {
            return null;
        }

        var firstArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (firstArgument == null)
        {
            return null;
        }

        var typeInfo = context.SemanticModel.GetTypeInfo(
            firstArgument.Expression);

        // использовать именно convertedType посоветовала нейронка с аргументом,
        // что это более надежно в сложных выражениях - требует уточнения
        var symbol = typeInfo.ConvertedType as INamedTypeSymbol;

        if (symbol == null ||
            symbol.TypeKind == TypeKind.Error ||
            symbol.SpecialType != SpecialType.None ||
            symbol.Name.EndsWith("Pattern"))
        {
            return null;
        }

        return symbol;
    }

    private static INamedTypeSymbol? GetTargetsByAttribute(
        GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        var hasAttribute = typeDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attr => attr.Name.ToString().EndsWith("AutoPattern"));

        if (!hasAttribute) 
        {
            return null;
        }

        return context.SemanticModel.GetDeclaredSymbol(typeDeclaration)
            as INamedTypeSymbol;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> symbols,
        SourceProductionContext context)
    {
        foreach (var typeSymbol in symbols)
        {
            var analysis = TypeAnalyzer.Analyze(typeSymbol, compilation);
            if (!analysis.IsValid)
            {
                continue;
            }

            var sourceCode = PatternCodeGenerator.Generate(analysis);
            context.AddSource($"{typeSymbol.Name}Pattern.g.cs", sourceCode);

            var extensionCode = GenerateAssertExtension(typeSymbol, compilation.Assembly);
            context.AddSource($"{typeSymbol.Name}AssertExtension.g.cs", extensionCode);
        }
    }

    private static string GenerateAssertExtension(
        INamedTypeSymbol typeSymbol,
        IAssemblySymbol currentAssembly)
    {
        var typeName = typeSymbol.Name;
        var fullTypeName = typeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat);
        var patternName = $"{typeName}Pattern";
        
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace 
                ? "GlobalExtensions" 
                : typeSymbol.ContainingNamespace.ToDisplayString();

        return $@"// <auto-generated />
using System;

namespace {ns}
{{
    public static partial class {typeName}AssertExtensions
    {{
        public static void Assert(
            this {fullTypeName} target,
            {fullTypeName} expected,
            [global::System.Runtime.CompilerServices.CallerArgumentExpression(""target"")] string? path = null)
        {{
            // Создаем паттерн (он сгенерирован в этой же сборке тестов)
            var pattern = {patternName}.Create(expected);
            global::PotternMotching.Ossertions.Assert(target, pattern, path);
        }}
    }}
}}";
    }
}
