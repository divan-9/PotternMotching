namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using PotternMotching.TestExternalModels;
using PotternMotching.Tests.ExternalPatterns;
using Xunit;
using CollectionPatternFactory = PotternMotching.Patterns.CollectionPattern;

public class NegativeGeneratedPatternTests
{
    [Fact]
    public void ScalarProperty_AcceptsNegatedValuePattern()
    {
        var pattern = new ExternalUserDtoPattern(Name: Pattern.Not(ValuePattern.Exact("Bob")));
        var value = new ExternalUserDto("42", "Alice", [], new ExternalAddress("London", "SW1"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NestedProperty_WhenNegatedPatternMatches_ReturnsFailureWithPropertyPath()
    {
        var pattern = new ExternalUserDtoPattern(
            Address: Pattern.Not(new ExternalAddressPattern(City: "London")));
        var value = new ExternalUserDto("42", "Alice", [], new ExternalAddress("London", "SW1"));

        var result = pattern.Evaluate(value);

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Address: [Pattern.Not]", Assert.Single(failure.Reasons));
    }

    [Fact]
    public void SequenceProperty_AcceptsNegatedCollectionPattern()
    {
        var pattern = new ExternalCollectionsDtoPattern(
            Roles: Pattern.Not(CollectionPatternFactory.AnyElement("admin")));
        var value = new ExternalCollectionsDto(["guest"], [], [], []);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void SetProperty_AcceptsNegatedCollectionPattern()
    {
        var pattern = new ExternalCollectionsDtoPattern(
            Flags: Pattern.Not(CollectionPatternFactory.AnyElement("disabled")));
        var value = new ExternalCollectionsDto([], [], ["active"], []);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void DictionaryProperty_AcceptsNegatedDictionaryPattern()
    {
        var pattern = new ExternalCollectionsDtoPattern(
            Scores: Pattern.Not(DictionaryPattern.Items(new Dictionary<string, int>
            {
                ["quality"] = 0,
            })));
        var value = new ExternalCollectionsDto([], [], [], new Dictionary<string, int>
        {
            ["quality"] = 10,
        });

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NullableSequenceProperty_NegatedCollectionPatternMatchesNull()
    {
        var pattern = new ExternalNullableCollectionPattern(
            Names: Pattern.Not(CollectionPatternFactory.AnyElement("admin")));
        var value = new ExternalNullableCollection("42", null, null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NullableNestedReferenceProperty_AdaptedNegationMatchesNull()
    {
        var pattern = new ExternalFieldOptionsPattern.Image(
            Options: Pattern.Not(new NullablePattern<ExternalMediaOptions>(
                new ExternalMediaOptionsPattern(Width: 100))));
        var value = new ExternalFieldOptions.Image("hero", null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NullableNestedReferenceProperty_NegatedExactNullRejectsNull()
    {
        var pattern = new ExternalFieldOptionsPattern.Image(
            Options: Pattern.Not(ValuePattern.Exact<ExternalMediaOptions?>(null)));
        var value = new ExternalFieldOptions.Image("hero", null);

        var result = pattern.Evaluate(value);

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Options: [Pattern.Not]", Assert.Single(failure.Reasons));
    }
}
