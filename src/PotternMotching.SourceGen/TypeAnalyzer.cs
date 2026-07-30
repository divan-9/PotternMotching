namespace PotternMotching.SourceGen;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PotternMotching.SourceGen.Models;

internal static class TypeAnalyzer
{
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
        var syntaxRef = parameter.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return NullableAnnotation.None;

        var syntax = syntaxRef.GetSyntax();
        if (syntax is not ParameterSyntax paramSyntax)
            return NullableAnnotation.None;

        return paramSyntax.Type is NullableTypeSyntax
            ? NullableAnnotation.Annotated
            : NullableAnnotation.None;
    }

    private static string GetTypeDisplayString(ITypeSymbol typeSymbol, NullableAnnotation nullableAnnotation)
    {
        var format = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        var baseType = typeSymbol.ToDisplayString(format);

        if (typeSymbol.IsReferenceType &&
            nullableAnnotation == NullableAnnotation.Annotated &&
            !baseType.EndsWith("?"))
        {
            return baseType + "?";
        }

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return baseType;
        }

        return baseType;
    }

    /// <summary>
    /// Gets the fully-qualified display string for a type symbol, ensuring that
    /// reference-type nullable annotations on type arguments are preserved.
    /// Unlike <see cref="GetTypeDisplayString"/>, this uses the type symbol's own
    /// <see cref="ITypeSymbol.NullableAnnotation"/> instead of a caller-provided annotation.
    /// </summary>
    private static string GetTypeArgumentDisplayString(ITypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.ToDisplayString(FullyQualifiedFormatWithNullability);

        if (typeSymbol.IsReferenceType &&
            typeSymbol.NullableAnnotation == NullableAnnotation.Annotated &&
            !baseType.EndsWith("?"))
        {
            return baseType + "?";
        }

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return baseType;
        }

        return baseType;
    }

    public static TypeAnalysisResult AnalyzeTargetType(
        INamedTypeSymbol typeSymbol,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes = null)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        if (typeSymbol.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.ExternalTargetMustBeClassOrRecord,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.ToDisplayString()));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        if (IsUnsupportedExternalTarget(typeSymbol) ||
            IsLessAccessibleThanInternal(typeSymbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.UnsupportedExternalTarget,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.ToDisplayString()));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            return AnalyzeStructType(typeSymbol, diagnostics, knownPatternTypes);
        }

        if (IsUnionType(typeSymbol))
        {
            return AnalyzeUnionType(typeSymbol, diagnostics, knownPatternTypes);
        }

        return typeSymbol.IsRecord
            ? AnalyzeRecordType(typeSymbol, diagnostics, knownPatternTypes)
            : AnalyzeClassLikeType(typeSymbol, diagnostics, knownPatternTypes);
    }

    private static TypeAnalysisResult AnalyzeRecordType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var enableDebug = false;

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

        var primaryConstructor = typeSymbol.Constructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        if (primaryConstructor is null)
        {
            return new TypeAnalysisResult(
                typeSymbol,
                true,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        var properties = ImmutableArray.CreateBuilder<PropertyAnalysisResult>();
        foreach (var parameter in primaryConstructor.Parameters)
        {
            properties.Add(AnalyzeProperty(parameter, diagnostics, enableDebug, knownPatternTypes));
        }

        var analyzedProperties = properties.ToImmutable();
        var hasParameterNameCollision = AddPatternParameterNameCollisionDiagnostics(
            typeSymbol: typeSymbol,
            properties: analyzedProperties,
            diagnostics: diagnostics);

        return new TypeAnalysisResult(
            typeSymbol,
            !hasParameterNameCollision,
            analyzedProperties,
            diagnostics.ToImmutable());
    }

    private static TypeAnalysisResult AnalyzeClassLikeType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        return AnalyzePublicReadablePropertiesType(
            typeSymbol: typeSymbol,
            diagnostics: diagnostics,
            knownPatternTypes: knownPatternTypes);
    }

    private static TypeAnalysisResult AnalyzeStructType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        return AnalyzePublicReadablePropertiesType(
            typeSymbol: typeSymbol,
            diagnostics: diagnostics,
            knownPatternTypes: knownPatternTypes);
    }

    private static TypeAnalysisResult AnalyzePublicReadablePropertiesType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var enableDebug = false;
        var properties = ImmutableArray.CreateBuilder<PropertyAnalysisResult>();

        foreach (var propertySymbol in GetPublicReadableInstanceProperties(typeSymbol))
        {
            properties.Add(AnalyzeProperty(propertySymbol, diagnostics, enableDebug, knownPatternTypes));
        }

        var analyzedProperties = properties.ToImmutable();
        var hasParameterNameCollision = AddPatternParameterNameCollisionDiagnostics(
            typeSymbol: typeSymbol,
            properties: analyzedProperties,
            diagnostics: diagnostics);

        return new TypeAnalysisResult(
            typeSymbol,
            !hasParameterNameCollision,
            analyzedProperties,
            diagnostics.ToImmutable());
    }

    private static TypeAnalysisResult AnalyzeUnionType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var variantTypes = typeSymbol.GetTypeMembers()
            .Where(t => t.IsRecord)
            .ToList();

        if (!variantTypes.Any())
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.UnsupportedExternalTarget,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.ToDisplayString()));

            return new TypeAnalysisResult(
                typeSymbol,
                false,
                ImmutableArray<PropertyAnalysisResult>.Empty,
                diagnostics.ToImmutable());
        }

        var variants = ImmutableArray.CreateBuilder<VariantAnalysisResult>();
        var hasParameterNameCollision = false;
        foreach (var variantType in variantTypes)
        {
            var variant = AnalyzeVariant(variantType, diagnostics, knownPatternTypes);
            variants.Add(variant);
            hasParameterNameCollision |= AddPatternParameterNameCollisionDiagnostics(
                typeSymbol: variantType,
                properties: variant.Properties,
                diagnostics: diagnostics);
        }

        return new TypeAnalysisResult(
            typeSymbol,
            !hasParameterNameCollision,
            ImmutableArray<PropertyAnalysisResult>.Empty,
            diagnostics.ToImmutable(),
            isUnion: true,
            variants: variants.ToImmutable());
    }

    private static ImmutableArray<IPropertySymbol> GetPublicReadableInstanceProperties(INamedTypeSymbol typeSymbol)
    {
        var orderedProperties = new List<IPropertySymbol>();
        var hierarchy = new Stack<INamedTypeSymbol>();

        for (var current = typeSymbol;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            hierarchy.Push(current);
        }

        while (hierarchy.Count > 0)
        {
            var current = hierarchy.Pop();

            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!IsPublicReadableInstanceProperty(property))
                    continue;

                var existingIndex = orderedProperties.FindIndex(p => p.Name == property.Name);
                if (existingIndex >= 0)
                {
                    orderedProperties[existingIndex] = property;
                }
                else
                {
                    orderedProperties.Add(property);
                }
            }
        }

        return [.. orderedProperties];
    }

    private static bool IsPublicReadableInstanceProperty(IPropertySymbol property)
    {
        return property.GetMethod?.DeclaredAccessibility == Accessibility.Public &&
               !property.IsStatic &&
               !property.IsIndexer;
    }

    private static PropertyAnalysisResult AnalyzeProperty(
        IParameterSymbol parameter,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        bool enableDebug,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var nullableAnnotation = GetNullableAnnotationFromSyntax(parameter);

        if (nullableAnnotation == NullableAnnotation.None)
        {
            nullableAnnotation = parameter.NullableAnnotation != NullableAnnotation.None
                ? parameter.NullableAnnotation
                : parameter.Type.NullableAnnotation;
        }

        return AnalyzeProperty(
            parameter.Name,
            parameter.Type,
            nullableAnnotation,
            parameter.Locations.FirstOrDefault(),
            diagnostics,
            enableDebug,
            knownPatternTypes);
    }

    private static PropertyAnalysisResult AnalyzeProperty(
        IPropertySymbol propertySymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        bool enableDebug,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var nullableAnnotation = propertySymbol.NullableAnnotation != NullableAnnotation.None
            ? propertySymbol.NullableAnnotation
            : propertySymbol.Type.NullableAnnotation;

        return AnalyzeProperty(
            propertySymbol.Name,
            propertySymbol.Type,
            nullableAnnotation,
            propertySymbol.Locations.FirstOrDefault(),
            diagnostics,
            enableDebug,
            knownPatternTypes);
    }

    private static PropertyAnalysisResult AnalyzeProperty(
        string propertyName,
        ITypeSymbol propertyType,
        NullableAnnotation nullableAnnotation,
        Location? location,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        bool enableDebug,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var propertyTypeString = GetTypeDisplayString(propertyType, nullableAnnotation);

        if (enableDebug)
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.DebugPropertyType,
                location,
                propertyName,
                propertyType.ToDisplayString(),
                nullableAnnotation.ToString(),
                propertyTypeString));
        }

        if (propertyType is IArrayTypeSymbol arrayType)
        {
            var elementType = arrayType.ElementType;
            var (requiresPattern, nestedType, nestedPatternType) = CheckForNestedPattern(elementType, knownPatternTypes);

            return new PropertyAnalysisResult(
                propertyName,
                propertyTypeString,
                PatternWrapperKind.Sequence,
                GetTypeArgumentDisplayString(elementType),
                null,
                null,
                requiresPattern,
                nestedType,
                nestedPatternType,
                elementType as INamedTypeSymbol);
        }

        if (propertyType is INamedTypeSymbol nullableValueType &&
            nullableValueType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            var underlyingType = nullableValueType.TypeArguments[0];
            var (isNested, nestedType, nestedPatternType) = CheckForNestedPattern(underlyingType, knownPatternTypes);

            if (isNested)
            {
                return new PropertyAnalysisResult(
                    propertyName,
                    propertyTypeString,
                    PatternWrapperKind.Nested,
                    null,
                    null,
                    null,
                    false,
                    nestedType,
                    nestedPatternType,
                    nullableValueType);
            }
        }

        if (propertyType is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            var typeFullName = namedType.OriginalDefinition.ToDisplayString();

            if (typeFullName == "System.Collections.Generic.HashSet<T>")
            {
                var setElementType = namedType.TypeArguments[0];
                var (requiresPattern, nestedTypeSymbol, nestedPatternType) = CheckForNestedPattern(setElementType, knownPatternTypes);

                return new PropertyAnalysisResult(
                    propertyName,
                    propertyTypeString,
                    PatternWrapperKind.Set,
                    GetTypeArgumentDisplayString(setElementType),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    nestedPatternType,
                    setElementType as INamedTypeSymbol);
            }

            if (ImplementsInterface(namedType, "System.Collections.Generic.ISet`1"))
            {
                var setElementType = namedType.TypeArguments[0];
                var (requiresPattern, nestedTypeSymbol, nestedPatternType) = CheckForNestedPattern(setElementType, knownPatternTypes);

                return new PropertyAnalysisResult(
                    propertyName,
                    propertyTypeString,
                    PatternWrapperKind.Set,
                    GetTypeArgumentDisplayString(setElementType),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    nestedPatternType,
                    setElementType as INamedTypeSymbol);
            }

            if (typeFullName == "System.Collections.Generic.Dictionary<TKey, TValue>" ||
                typeFullName == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>" ||
                ImplementsInterface(namedType, "System.Collections.Generic.IDictionary`2") ||
                ImplementsInterface(namedType, "System.Collections.Generic.IReadOnlyDictionary`2"))
            {
                if (namedType.TypeArguments.Length >= 2)
                {
                    var keyType = namedType.TypeArguments[0];
                    var valueType = namedType.TypeArguments[1];
                    return new PropertyAnalysisResult(
                        propertyName,
                        propertyTypeString,
                        PatternWrapperKind.Dictionary,
                        null,
                        GetTypeArgumentDisplayString(keyType),
                        GetTypeArgumentDisplayString(valueType),
                        propertyTypeSymbol: namedType);
                }
            }

            if (TryGetEnumerableElementType(namedType, out var enumerableElementType) &&
                enumerableElementType is not null)
            {
                var (requiresPattern, nestedTypeSymbol, nestedPatternType) = CheckForNestedPattern(enumerableElementType, knownPatternTypes);

                return new PropertyAnalysisResult(
                    propertyName,
                    propertyTypeString,
                    PatternWrapperKind.Sequence,
                    GetTypeArgumentDisplayString(enumerableElementType),
                    null,
                    null,
                    requiresPattern,
                    nestedTypeSymbol,
                    nestedPatternType,
                    enumerableElementType as INamedTypeSymbol);
            }
        }

        if (propertyType is INamedTypeSymbol namedTypeNoArgs)
        {
            var (isNested, nestedType, nestedPatternType) = CheckForNestedPattern(namedTypeNoArgs, knownPatternTypes);
            if (isNested)
            {
                return new PropertyAnalysisResult(
                    propertyName,
                    propertyTypeString,
                    PatternWrapperKind.Nested,
                    null,
                    null,
                    null,
                    false,
                    nestedType,
                    nestedPatternType,
                    namedTypeNoArgs);
            }
        }

        return new PropertyAnalysisResult(
            propertyName,
            propertyTypeString,
            PatternWrapperKind.Value,
            propertyTypeSymbol: propertyType);
    }

    private static bool AddPatternParameterNameCollisionDiagnostics(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<PropertyAnalysisResult> properties,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        var groups = properties
            .GroupBy(property => GetPatternParameterName(property.PropertyName), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToList();

        foreach (var group in groups)
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.GeneratedPatternParameterNameCollision,
                typeSymbol.Locations.FirstOrDefault(),
                group.Key,
                typeSymbol.ToDisplayString()));
        }

        return groups.Count > 0;
    }

    private static string GetPatternParameterName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
    }

    private static bool TryGetEnumerableElementType(
        INamedTypeSymbol type,
        out ITypeSymbol? elementType)
    {
        if (type.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
        {
            elementType = type.TypeArguments[0];
            return true;
        }

        var enumerableInterface = type.AllInterfaces.FirstOrDefault(i =>
            i.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>");

        if (enumerableInterface is null)
        {
            elementType = null;
            return false;
        }

        elementType = enumerableInterface.TypeArguments[0];
        return true;
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceName)
    {
        return type.AllInterfaces.Any(i =>
        {
            var name = i.OriginalDefinition.ToDisplayString();
            return name == interfaceName.Replace("`1", "<T>").Replace("`2", "<TKey, TValue>");
        });
    }

    private static (bool HasPattern, INamedTypeSymbol? TypeSymbol, string? PatternTypeName) CheckForNestedPattern(
        ITypeSymbol type,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return (false, null, null);
        }

        if (knownPatternTypes != null && knownPatternTypes.TryGetValue(namedType, out var knownPatternType))
        {
            return (true, namedType, knownPatternType);
        }

        var containingType = namedType.ContainingType;
        if (containingType != null)
        {
            var hasUnionAttribute = containingType.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Contains(UnionAttributeName) ?? false);

            if (hasUnionAttribute &&
                knownPatternTypes != null &&
                knownPatternTypes.TryGetValue(containingType, out var unionPatternType))
            {
                return (true, containingType, unionPatternType);
            }
        }

        return (false, null, null);
    }

    private static bool IsUnionType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Contains(UnionAttributeName) ?? false);
    }

    private static bool IsUnsupportedExternalTarget(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
               typeSymbol.IsUnboundGenericType ||
               typeSymbol.TypeArguments.Any(ContainsOpenTypeComponent);
    }

    private static bool IsLessAccessibleThanInternal(INamedTypeSymbol typeSymbol)
    {
        for (INamedTypeSymbol? current = typeSymbol; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is Accessibility.Private or
                Accessibility.Protected or
                Accessibility.ProtectedAndInternal)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsOpenTypeComponent(ITypeSymbol typeSymbol)
    {
        return typeSymbol switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol arrayType => ContainsOpenTypeComponent(arrayType.ElementType),
            IPointerTypeSymbol pointerType => ContainsOpenTypeComponent(pointerType.PointedAtType),
            INamedTypeSymbol namedType => namedType.IsUnboundGenericType || namedType.TypeArguments.Any(ContainsOpenTypeComponent),
            _ => false,
        };
    }

    private static TypeAnalysisResult AnalyzeUnion(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
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

        var variantTypes = typeSymbol.GetTypeMembers()
            .Where(t => t.IsRecord)
            .ToList();

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

        var variants = ImmutableArray.CreateBuilder<VariantAnalysisResult>();
        foreach (var variantType in variantTypes)
        {
            variants.Add(AnalyzeVariant(variantType, diagnostics, knownPatternTypes));
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
        ImmutableArray<Diagnostic>.Builder diagnostics,
        ImmutableDictionary<INamedTypeSymbol, string>? knownPatternTypes)
    {
        var enableDebug = false;

        var primaryConstructor = variantSymbol.Constructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        if (primaryConstructor is null)
        {
            return new VariantAnalysisResult(
                variantSymbol,
                ImmutableArray<PropertyAnalysisResult>.Empty);
        }

        var properties = ImmutableArray.CreateBuilder<PropertyAnalysisResult>();
        foreach (var parameter in primaryConstructor.Parameters)
        {
            properties.Add(AnalyzeProperty(parameter, diagnostics, enableDebug, knownPatternTypes));
        }

        return new VariantAnalysisResult(variantSymbol, properties.ToImmutable());
    }
}
