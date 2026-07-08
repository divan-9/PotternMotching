namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using PotternMotching.TestExternalModels;
using PotternMotching.Tests.ExternalPatterns;
using Xunit;

public class ExternalAutoPatternTests
{
    [Fact]
    public void ExternalRecord_GeneratesPatternAndMatches()
    {
        var user = new ExternalUserDto(
            Id: "42",
            Name: "Alice",
            Roles: ["admin", "editor"],
            Address: new ExternalAddress("Seattle", "98101"));

        var pattern = new ExternalUserDtoPattern(
            Id: "42",
            Address: new ExternalAddressPattern(City: "Seattle"));

        var result = pattern.Evaluate(user);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_CollectionsUseExistingWrapperRules()
    {
        var dto = new ExternalCollectionsDto(
            Roles: ["admin", "editor"],
            Numbers: [1, 2, 3],
            Flags: ["beta", "dark-mode"],
            Scores: new Dictionary<string, int>
            {
                ["quality"] = 10,
                ["speed"] = 8,
            });

        var pattern = new ExternalCollectionsDtoPattern(
            Roles: ["admin", "editor"],
            Numbers: [1, 2, 3],
            Flags: ["beta"],
            Scores: DictionaryPattern.Items(new Dictionary<string, int>
            {
                ["quality"] = 10,
            }));

        var result = pattern.Evaluate(dto);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_ImplicitConversionToPattern_Works()
    {
        ExternalAddressPattern pattern = new ExternalAddress("Seattle", "98101");

        var result = pattern.Evaluate(new ExternalAddress("Seattle", "98101"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_NestedUnknownRecord_FallsBackToValueMatching()
    {
        var value = new ExternalWrappedUnknown(
            Id: "42",
            Unknown: new ExternalUnknown("hello"));

        var pattern = new ExternalWrappedUnknownPattern(
            Unknown: new ExternalUnknown("hello"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClass_PublicReadableProperties_AreGeneratedAndMatched()
    {
        var value = new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
            Hidden = "secret",
        };

        var pattern = new ExternalClassDtoPattern(
            Id: "42",
            Name: "Alice",
            RoleCount: 2,
            Address: new ExternalAddressPattern(City: "Seattle"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClass_ImplicitConversionToPattern_Works()
    {
        ExternalClassDtoPattern pattern = new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
        };

        var result = pattern.Evaluate(new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClosedGenericRecord_GeneratesPattern()
    {
        var value = new ExternalGenericBox<ExternalAddress>(
            Value: new ExternalAddress("Seattle", "98101"));

        var pattern = new ExternalGenericBox_ExternalAddressPattern(
            Value: new ExternalAddressPattern(City: "Seattle"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClosedGenericRecord_NestedPattern_UsesConstructedGenericPattern()
    {
        var value = new ExternalGenericEnvelope<ExternalAddress>(
            Id: "42",
            Box: new ExternalGenericBox<ExternalAddress>(
                Value: new ExternalAddress("Seattle", "98101")));

        var pattern = new ExternalGenericEnvelope_ExternalAddressPattern(
            Id: "42",
            Box: new ExternalGenericBox_ExternalAddressPattern(
                Value: new ExternalAddressPattern(City: "Seattle")));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalPolymorphicProperty_AcceptsConcreteGeneratedPattern()
    {
        var value = new ExternalImpressionRule(
            Id: "42",
            FragmentTemplate: new ExternalStringFragmentTemplate("xxxx"));

        var pattern = new ExternalImpressionRulePattern(
            Id: "42",
            FragmentTemplate: new ExternalStringFragmentTemplatePattern(Value: "xxxx"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalPolymorphicProperty_WholeObjectImplicitConversion_Works()
    {
        ExternalImpressionRulePattern pattern = new ExternalImpressionRule(
            Id: "42",
            FragmentTemplate: new ExternalStringFragmentTemplate("xxxx"));

        var result = pattern.Evaluate(new ExternalImpressionRule(
            Id: "42",
            FragmentTemplate: new ExternalStringFragmentTemplate("xxxx")));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_RootPattern_Works()
    {
        ExternalJobPattern pattern = new ExternalJobPattern.Employed(
            Company: "Tech Corp",
            Position: "Developer");

        var result = pattern.Evaluate(new ExternalJob.Employed("Tech Corp", "Developer"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_ImplicitConversionToBasePattern_Works()
    {
        ExternalJobPattern pattern = new ExternalJob.Employed("Tech Corp", "Developer");

        var result = pattern.Evaluate(new ExternalJob.Employed("Tech Corp", "Developer"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_NestedInExternalRecord_UsesGeneratedPattern()
    {
        var value = new ExternalJobApplication(
            CompanyName: "Acme Corp",
            DesiredPosition: new ExternalJob.Employed("Tech Corp", "Engineer"));

        var pattern = new ExternalJobApplicationPattern(
            CompanyName: "Acme Corp",
            DesiredPosition: new ExternalJobPattern.Employed(
                Company: "Tech Corp",
                Position: "Engineer"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_EmptyVariant_Works()
    {
        ExternalJobPattern pattern = new ExternalJob.Unemployed();

        var result = pattern.Evaluate(new ExternalJob.Unemployed());

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_MismatchVariant_ReturnsHelpfulFailure()
    {
        var pattern = new ExternalJobPattern.Employed(
            Company: "Tech Corp",
            Position: "Developer");

        var result = pattern.Evaluate(new ExternalJob.Unemployed(), ".Job");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Job", failure.Reasons[0]);
        Assert.Contains("Expected variant Employed", failure.Reasons[0]);
        Assert.Contains("Unemployed", failure.Reasons[0]);
    }

    [Fact]
    public void ExternalUnion_VariantCollectionInExternalRecord_UsesSpecificVariantPatternType()
    {
        var value = new ExternalCompany(
            Name: "Tech Corp",
            Employees:
            [
                new ExternalJob.Employed("Tech Corp", "Engineer"),
                new ExternalJob.Employed("Tech Corp", "Manager")
            ]);

        var pattern = new ExternalCompanyPattern(
            Name: "Tech Corp",
            Employees:
            [
                new ExternalJobPattern.Employed(Company: "Tech Corp", Position: "Engineer"),
                new ExternalJobPattern.Employed(Company: "Tech Corp", Position: "Manager")
            ]);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_WithKeywordVariantNames_Works()
    {
        ExternalContentPattern pattern = new ExternalContent.String(Value: "hello");

        var result = pattern.Evaluate(new ExternalContent.String("hello"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_WithKeywordVariantNames_MismatchReportsActualVariant()
    {
        var pattern = new ExternalContentPattern.Object(Id: "42");

        var result = pattern.Evaluate(new ExternalContent.String("hello"), ".Content");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Content", failure.Reasons[0]);
        Assert.Contains("Expected variant Object", failure.Reasons[0]);
        Assert.Contains("String", failure.Reasons[0]);
    }

    // ──── Nullable collection regression tests ────

    /// <summary>
    /// Issue 1: Implicit conversion from a value with a null collection property
    /// must not throw NullReferenceException.
    /// </summary>
    [Fact]
    public void NullableCollection_ImplicitConversion_WithNullCollection_DoesNotCrash()
    {
        var value = new ExternalNullableCollection(
            Id: "42",
            Names: null,
            Scores: null);

        // This call will go through the generated implicit conversion which calls
        // SequencePatternDefault.From(null) and DictionaryPatternDefault.From(null).
        // Both would NRE without the fix (Issue 1).
        var exception = Record.Exception(() =>
        {
            ExternalNullableCollectionPattern pattern = value;
        });

        Assert.Null(exception);
    }

    /// <summary>
    /// Issue 1: A pattern created from a value with null collections should
    /// match another value (collections default to "match any").
    /// </summary>
    [Fact]
    public void NullableCollection_ImplicitConversion_MatchesAnyValue()
    {
        var source = new ExternalNullableCollection(
            Id: "42",
            Names: null,
            Scores: null);

        ExternalNullableCollectionPattern pattern = source;

        var target = new ExternalNullableCollection(
            Id: "42",
            Names: ["Alice", "Bob"],
            Scores: new Dictionary<string, int> { ["q"] = 10 });

        var result = pattern.Evaluate(target);
        Assert.IsType<MatchResult.Success>(result);
    }

    /// <summary>
    /// Issue 1: Implicit conversion with a non-null collection works correctly.
    /// </summary>
    [Fact]
    public void NullableCollection_ImplicitConversion_WithNonNullCollection_Matches()
    {
        var value = new ExternalNullableCollection(
            Id: "1",
            Names: ["Alice", "Bob"],
            Scores: new Dictionary<string, int> { ["q"] = 10 });

        ExternalNullableCollectionPattern pattern = value;

        var result = pattern.Evaluate(value);
        Assert.IsType<MatchResult.Success>(result);
    }

    /// <summary>
    /// Issue 2: Nullable elements inside a collection (List&lt;string?&gt;).
    /// The generated pattern SHOULD use ValuePattern&lt;string?&gt;
    /// so that null elements are handled correctly.
    /// Currently the generator loses the type argument nullability,
    /// producing ValuePattern&lt;string&gt; (non-nullable), so null elements
    /// cannot be expressed in pattern literals and may behave incorrectly.
    /// After the fix, element type should be <c>string?</c>.
    /// </summary>
    [Fact]
    public void NullableElements_WithNullElementInSource_ImplicitConversionWorks()
    {
        // Source has a null element in Tags (List<string?>).
        var value = new ExternalNullableElements(
            Id: "42",
            Tags: ["alpha", null, "gamma"]);

        // Implicit conversion should not crash even with null elements.
        var exception = Record.Exception(() =>
        {
            ExternalNullableElementsPattern pattern = value;
        });

        Assert.Null(exception);
    }

    /// <summary>
    /// Issue 2: Matching round-trip with nullable element in collection.
    /// After the fix, the generated pattern element type should be <c>string?</c>
    /// and this round-trip should succeed.
    /// </summary>
    [Fact]
    public void NullableElements_RoundTrip_Matches()
    {
        var value = new ExternalNullableElements(
            Id: "42",
            Tags: ["alpha", null, "gamma"]);

        ExternalNullableElementsPattern pattern = value;

        var result = pattern.Evaluate(value);
        Assert.IsType<MatchResult.Success>(result);
    }

    /// <summary>
    /// Issue 3: SetPatternDefault with <c>default</c> (unspecified)
    /// must not throw when evaluating. The inner matcher is null.
    /// </summary>
    [Fact]
    public void NullableSet_DefaultMatcher_DoesNotCrash()
    {
        var value = new ExternalNullableSet(
            Id: "42",
            Flags: ["beta", "gamma"]);

        // Flags is default → innerMatcher is null.
        // SetPatternDefault.Evaluate must handle null innerMatcher.
        var pattern = new ExternalNullableSetPattern(Id: "42");

        var exception = Record.Exception(() => pattern.Evaluate(value));
        Assert.Null(exception);
    }

    /// <summary>
    /// Issue 3: SetPatternDefault with <c>default</c> matches any value.
    /// </summary>
    [Fact]
    public void NullableSet_DefaultMatcher_MatchesAny()
    {
        var value = new ExternalNullableSet(
            Id: "42",
            Flags: ["beta", "gamma"]);

        var pattern = new ExternalNullableSetPattern(Id: "42");

        var result = pattern.Evaluate(value);
        Assert.IsType<MatchResult.Success>(result);
    }

    /// <summary>
    /// Issue 1+3: Implicit conversion from a value with null HashSet
    /// must not throw, and the resulting default pattern matches any.
    /// </summary>
    [Fact]
    public void NullableSet_ImplicitConversion_WithNullSet_DoesNotCrash()
    {
        var value = new ExternalNullableSet(
            Id: "42",
            Flags: null);

        var exception = Record.Exception(() =>
        {
            ExternalNullableSetPattern pattern = value;
        });

        Assert.Null(exception);
    }
}
