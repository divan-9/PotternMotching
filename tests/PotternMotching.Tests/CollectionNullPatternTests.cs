namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using Xunit;

public class CollectionNullPatternTests
{
    [Fact]
    public void SequenceNullMarker_MatchesNull()
    {
        var pattern = new NullableCollectionsPattern(Sequence: ValuePattern.Null());
        var value = new NullableCollections(Sequence: null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SequenceNullMarker_RejectsNonNullCollection(bool populated)
    {
        var pattern = new NullableCollectionsPattern(Sequence: ValuePattern.Null());
        var value = new NullableCollections(
            Sequence: populated ? [new NullableCollectionItem(Id: "entry")] : []);

        var result = pattern.Evaluate(value, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Value.Sequence", Assert.Single(failure.Reasons));
    }

    [Fact]
    public void SetNullMarker_MatchesNull()
    {
        var pattern = new NullableCollectionsPattern(Set: ValuePattern.Null());
        var value = new NullableCollections(Set: null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetNullMarker_RejectsNonNullCollection(bool populated)
    {
        var pattern = new NullableCollectionsPattern(Set: ValuePattern.Null());
        var value = new NullableCollections(
            Set: populated ? [new NullableCollectionItem(Id: "entry")] : []);

        var result = pattern.Evaluate(value, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Value.Set", Assert.Single(failure.Reasons));
    }

    [Fact]
    public void DictionaryNullMarker_MatchesNull()
    {
        var pattern = new NullableCollectionsPattern(Dictionary: ValuePattern.Null());
        var value = new NullableCollections(Dictionary: null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DictionaryNullMarker_RejectsNonNullDictionary(bool populated)
    {
        var pattern = new NullableCollectionsPattern(Dictionary: ValuePattern.Null());
        var items = new Dictionary<string, NullableCollectionItem>();
        if (populated)
        {
            items.Add("key", new NullableCollectionItem(Id: "entry"));
        }

        var value = new NullableCollections(Dictionary: items);

        var result = pattern.Evaluate(value, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Value.Dictionary", Assert.Single(failure.Reasons));
    }

    [Fact]
    public void ReadOnlyDictionaryNullMarker_MatchesNull()
    {
        var pattern = new NullableCollectionsPattern(ReadOnlyDictionary: ValuePattern.Null());
        var value = new NullableCollections(ReadOnlyDictionary: null);

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadOnlyDictionaryNullMarker_RejectsNonNullDictionary(bool populated)
    {
        var pattern = new NullableCollectionsPattern(ReadOnlyDictionary: ValuePattern.Null());
        var items = new Dictionary<string, NullableCollectionItem>();
        if (populated)
        {
            items.Add("key", new NullableCollectionItem(Id: "entry"));
        }

        var value = new NullableCollections(ReadOnlyDictionary: items);

        var result = pattern.Evaluate(value, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Value.ReadOnlyDictionary", Assert.Single(failure.Reasons));
    }
}

public record NullableCollectionItem(string Id);

public record NullableCollections(
    NullableCollectionItem[]? Sequence = null,
    HashSet<NullableCollectionItem>? Set = null,
    Dictionary<string, NullableCollectionItem>? Dictionary = null,
    IReadOnlyDictionary<string, NullableCollectionItem>? ReadOnlyDictionary = null);

[AutoPatternFor(typeof(NullableCollectionItem))]
[AutoPatternFor(typeof(NullableCollections))]
internal static class Patterns;
