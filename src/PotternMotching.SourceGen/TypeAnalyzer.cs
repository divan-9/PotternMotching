namespace PotternMotching.SourceGen;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PotternMotching.SourceGen.Models;

internal static class TypeAnalyzer
{
    private const string AutoPatternAttributeName = "PotternMotching.AutoPatternAttribute";
    private const string UnionAttributeName = "Dunet.UnionAttribute";

    // Custom format that includes nullable annotations
    private static readonly SymbolDisplayFormat FullyQualifiedFormatWithNullability = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static NullableAnnotation GetNullableAnnotationFromSyntax(IParameterSymbol parameter)
    {
        // Get the syntax node for this parameter
        var syntaxRef = parameter.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return NullableAnnotation.None;

        var syntax = syntaxRef.GetSyntax();
        if (syntax is not ParameterSyntax paramSyntax)
            return NullableAnnotation.None;

        // Check if the type syntax has a nullable annotation (?)
        if (paramSyntax.Type is NullableTypeSyntax)
        {
            return NullableAnnotation.Annotated;
        }

        return NullableAnnotation.None;
    }

    private static string GetTypeDisplayString(ITypeSymbol typeSymbol, NullableAnnotation nullableAnnotation)
    {
        // Use a custom format that includes nullable reference type modifiers
        var format = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        var baseType = typeSymbol.ToDisplayString(format);

        // For reference types with nullable annotation, add the ? suffix if not already present
        // This handles cases where the format doesn't include it
        if (typeSymbol.IsReferenceType &&
            nullableAnnotation == NullableAnnotation.Annotated &&
            !baseType.EndsWith("?"))
        {
            return baseType + "?";
        }

        // For nullable value types (Nullable<T>), the format should already include it
        // but let's make sure
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            // This is already Nullable<T>, which ToDisplayString represents as T?
            return baseType;
        }

        return baseType;
    }

    public static TypeAnalysisResult Analyze(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var enableDebug = false; // Set to true to enable debug output

        // Check if it's a union type
        var isUnion = IsUnionType(typeSymbol);

        if (isUnion)
        {
            return AnalyzeUnion(typeSymbol, compilation, diagnostics);
        }

        // Validate it's a record
        if (!typeSymbol.IsRecord)
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.TypeMustBeRecord,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        // Validate no inheritance (except from object)
        if (typeSymbol.BaseType is not null &&
            typeSymbol.BaseType.SpecialType != SpecialType.System_Object)
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.InheritanceNotSupported,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        // Get primary constructor parameters
        var primaryConstructor = typeSymbol.Constructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        if (primaryConstructor is null)
        {
            // Empty record is valid, just has no properties to match
            return new TypeAnalysisResult(
                typeSymbol,
                true,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        // Analyze each parameter
        var properties = ImmutableArray.CreateBuilder<PropertyAnalysisResult>();
        foreach (var parameter in primaryConstructor.Parameters)
        {
            var property = AnalyzeProperty(parameter, compilation, diagnostics, enableDebug);
            properties.Add(property);
        }

        return new TypeAnalysisResult(
            typeSymbol,
            true,
            properties.ToImmutable(),
            diagnostics.ToImmutable());
    }

    private static PropertyAnalysisResult AnalyzeProperty(
        IParameterSymbol parameter,
        Compilation compilation,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        bool enableDebug)
    {
        var propertyType = parameter.Type;

        // Check if the parameter has a nullable annotation by examining the syntax
        var nullableAnnotation = GetNullableAnnotationFromSyntax(parameter);

        if (nullableAnnotation == NullableAnnotation.None)
        {
            // Fallback to semantic model
            nullableAnnotation = parameter.NullableAnnotation != NullableAnnotation.None
                ? parameter.NullableAnnotation
                : propertyType.NullableAnnotation;
        }

        // Get the type display string with nullable annotation
        var propertyTypeString = GetTypeDisplayString(propertyType, nullableAnnotation);

        // Debug output
        if (enableDebug)
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.DebugPropertyType,
                parameter.Locations.FirstOrDefault(),
                parameter.Name,
                propertyType.ToDisplayString(),
                nullableAnnotation.ToString(),
                propertyTypeString));
        }

        // Check for arrays
        if (propertyType is IArrayTypeSymbol arrayType)
        {
            var elementType = arrayType.ElementType;
            var (requiresPattern, nestedType) = CheckForNestedPattern(elementType, compilation);

            return new PropertyAnalysisResult(
                parameter.Name,
                propertyTypeString,
                PatternWrapperKind.Sequence,
                elementType.ToDisplayString(FullyQualifiedFormatWithNullability),
                null,
                null,
                requiresPattern,
                nestedType,
                elementType as INamedTypeSymbol);
        }

        // Check for named types (generic collections, etc.)
        if (propertyType is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            var typeFullName = namedType.OriginalDefinition.ToDisplayString();

            // Check for HashSet<T> specifically first
            if (typeFullName == "System.Collections.Generic.HashSet<T>")
            {
                var elementType = namedType.TypeArguments[0];
                var (requiresPattern, nestedTypeSymbol) = CheckForNestedPattern(elementType, compilation);

                return new PropertyAnalysisResult(
                    parameter.Name,
                    propertyTypeString,
                    PatternWrapperKind.Set,
                    elementType.ToDisplayString(FullyQualifiedFormatWithNullability),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    elementType as INamedTypeSymbol);
            }

            // Check for ISet<T>
            if (ImplementsInterface(namedType, "System.Collections.Generic.ISet`1"))
            {
                var elementType = namedType.TypeArguments[0];
                var (requiresPattern, nestedTypeSymbol) = CheckForNestedPattern(elementType, compilation);

                return new PropertyAnalysisResult(
                    parameter.Name,
                    propertyTypeString,
                    PatternWrapperKind.Set,
                    elementType.ToDisplayString(FullyQualifiedFormatWithNullability),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    elementType as INamedTypeSymbol);
            }

            // Check for IDictionary<TKey, TValue> or Dictionary<TKey, TValue>
            if (typeFullName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
                ImplementsInterface(namedType, "System.Collections.Generic.IDictionary`2"))
            {
                if (namedType.TypeArguments.Length >= 2)
                {
                    var keyType = namedType.TypeArguments[0];
                    var valueType = namedType.TypeArguments[1];
                    return new PropertyAnalysisResult(
                        parameter.Name,
                        propertyTypeString,
                        PatternWrapperKind.Dictionary,
                        null,
                        keyType.ToDisplayString(FullyQualifiedFormatWithNullability),
                        valueType.ToDisplayString(FullyQualifiedFormatWithNullability));
                }
            }

            // Check for IEnumerable<T>, IList<T>, List<T>
            if (ImplementsInterface(namedType, "System.Collections.Generic.IEnumerable`1"))
            {
                var elementType = namedType.TypeArguments[0];
                var (requiresPattern, nestedTypeSymbol) = CheckForNestedPattern(elementType, compilation);

                return new PropertyAnalysisResult(
                    parameter.Name,
                    propertyTypeString,
                    PatternWrapperKind.Sequence,
                    elementType.ToDisplayString(FullyQualifiedFormatWithNullability),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    elementType as INamedTypeSymbol);
            }
        }

        // Check for named types without type arguments (potential nested patterns)
        if (propertyType is INamedTypeSymbol namedTypeNoArgs)
        {
            // Check if it's a nested type with [AutoPattern]
            var (isNested, nestedType) = CheckForNestedPattern(namedTypeNoArgs, compilation);
            if (isNested)
            {
                return new PropertyAnalysisResult(
                    parameter.Name,
                    propertyTypeString,
                    PatternWrapperKind.Nested,
                    null,
                    null,
                    null,
                    false,
                    nestedType,
                    namedTypeNoArgs);
            }
        }

        // Default: ValuePattern
        return new PropertyAnalysisResult(
            parameter.Name,
            propertyTypeString,
            PatternWrapperKind.Value);
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceName)
    {
        return type.AllInterfaces.Any(i =>
        {
            var name = i.OriginalDefinition.ToDisplayString();
            return name == interfaceName.Replace("`1", "<T>").Replace("`2", "<TKey, TValue>");
        });
    }

    private static bool IsType(INamedTypeSymbol type, string typeName)
    {
        var name = type.OriginalDefinition.ToDisplayString();
        return name == typeName.Replace("`1", "<T>").Replace("`2", "<TKey, TValue>");
    }

    private static (bool HasAutoPattern, INamedTypeSymbol? TypeSymbol) CheckForNestedPattern(
        ITypeSymbol type,
        Compilation compilation)
    {
        if (type is not INamedTypeSymbol namedType || !namedType.IsRecord)
        {
            return (false, null);
        }

        // Check if THIS type directly has [AutoPattern]
        var hasAttribute = namedType.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Contains(AutoPatternAttributeName) ?? false);

        if (hasAttribute)
        {
            return (true, namedType);
        }

        // Check if this type is a VARIANT of a union with [AutoPattern]
        var containingType = namedType.ContainingType;
        if (containingType != null && containingType.IsRecord)
        {
            // Check if containing type has [Union] attribute (from Dunet)
            var hasUnionAttribute = containingType.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Contains(UnionAttributeName) ?? false);

            // Check if containing type has [AutoPattern] attribute
            var hasContainingAutoPattern = containingType.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Contains(AutoPatternAttributeName) ?? false);

            if (hasUnionAttribute && hasContainingAutoPattern)
            {
                // This is a union variant - return the CONTAINING type (the union)
                // This allows us to construct the pattern type as JobPattern.Employed
                return (true, containingType);
            }
        }

        return (false, null);
    }

    private static bool IsUnionType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Contains(UnionAttributeName) ?? false);
    }

    private static TypeAnalysisResult AnalyzeUnion(
        INamedTypeSymbol typeSymbol,
        Compilation compilation,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        // Validate it's partial (required for Dunet)
        if (!typeSymbol.DeclaringSyntaxReferences.Any(r => r.GetSyntax().ToString().Contains("partial")))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.UnionMustBePartial,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        // Get nested type members (variants)
        var variantTypes = typeSymbol.GetTypeMembers()
            .Where(t => t.IsRecord)
            .ToList();

        // Validate at least one variant exists
        if (!variantTypes.Any())
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.UnionMustHaveVariants,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        // Analyze each variant
        var variants = ImmutableArray.CreateBuilder<VariantAnalysisResult>();
        foreach (var variantType in variantTypes)
        {
            var variant = AnalyzeVariant(variantType, compilation, diagnostics);
            variants.Add(variant);
        }

        return new TypeAnalysisResult(
            typeSymbol,
            true,
            ImmutableArray<PropertyAnalysisResult>.Empty,
            diagnostics.ToImmutable(),
            isUnion: true,
            variants: variants.ToImmutable());
    }

    private static VariantAnalysisResult AnalyzeVariant(
        INamedTypeSymbol variantSymbol,
        Compilation compilation,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        var enableDebug = false; // Match the debug setting from Analyze method

        // Get primary constructor
        var primaryConstructor = variantSymbol.Constructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        if (primaryConstructor is null)
        {
            // Empty variant is valid, just has no properties
            return new VariantAnalysisResult(
                variantSymbol,
                ImmutableArray<PropertyAnalysisResult>.Empty);
        }

        // Analyze each parameter using existing logic
        var properties = ImmutableArray.CreateBuilder<PropertyAnalysisResult>();
        foreach (var parameter in primaryConstructor.Parameters)
        {
            var property = AnalyzeProperty(parameter, compilation, diagnostics, enableDebug);
            properties.Add(property);
        }

        return new VariantAnalysisResult(variantSymbol, properties.ToImmutable());
    }
}
