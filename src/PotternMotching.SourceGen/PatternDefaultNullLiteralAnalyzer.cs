namespace PotternMotching.SourceGen;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Reports a diagnostic when a null literal is passed to a nullable <c>PatternDefault&lt;T, TPattern&gt;</c> parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PatternDefaultNullLiteralAnalyzer : DiagnosticAnalyzer
{
    private const string PatternDefaultMetadataName = "PotternMotching.Patterns.PatternDefault`2";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.NullLiteralRequiresNullableCast,
    ];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNullLiteral, SyntaxKind.NullLiteralExpression);
    }

    private static void AnalyzeNullLiteral(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LiteralExpressionSyntax nullLiteral)
        {
            return;
        }

        var argument = nullLiteral.FirstAncestorOrSelf<ArgumentSyntax>();
        if (argument is null)
        {
            return;
        }

        var parameter = ResolveParameter(argument, context.SemanticModel);
        if (parameter?.Type is not INamedTypeSymbol parameterType)
        {
            return;
        }

        var patternDefaultType = context.SemanticModel.Compilation.GetTypeByMetadataName(PatternDefaultMetadataName);
        if (patternDefaultType is null || !SymbolEqualityComparer.Default.Equals(parameterType.OriginalDefinition, patternDefaultType))
        {
            return;
        }

        var candidateType = parameterType.TypeArguments[0];
        if (!IsNullableType(candidateType))
        {
            return;
        }

        var argumentName = parameter.Name;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.NullLiteralRequiresNullableCast,
            nullLiteral.GetLocation(),
            argumentName));
    }

    private static IParameterSymbol? ResolveParameter(ArgumentSyntax argument, SemanticModel semanticModel)
    {
        if (semanticModel.GetOperation(argument) is IArgumentOperation argumentOperation && argumentOperation.Parameter is not null)
        {
            return argumentOperation.Parameter;
        }

        if (argument.Parent is not BaseArgumentListSyntax argumentList)
        {
            return null;
        }

        IMethodSymbol? target = argumentList.Parent switch
        {
            ObjectCreationExpressionSyntax objectCreation => semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol,
            ImplicitObjectCreationExpressionSyntax implicitObjectCreation => semanticModel.GetSymbolInfo(implicitObjectCreation).Symbol as IMethodSymbol,
            InvocationExpressionSyntax invocation => semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol,
            _ => null
        };

        if (target is null)
        {
            return null;
        }

        if (argument.NameColon is { Name.Identifier.ValueText: var name })
        {
            return target.Parameters.FirstOrDefault(parameter => parameter.Name == name);
        }

        var index = argumentList.Arguments.IndexOf(argument);
        return index >= 0 && index < target.Parameters.Length
            ? target.Parameters[index]
            : null;
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }

        return type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.Annotated;
    }
}
