namespace PotternMotching.Tests.DictionaryMatcher;

using PotternMotching.Patterns;
using Xunit;

public class MatchAllTests
{
    [Fact]
    public void EvaluateMatchAll_EmptyPatternMatchesAnyDictionary_ReturnsSuccess()
    {
        var matcher = DictionaryPattern.Items<string, int>(new()
        {

        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 1,
            ["key2"] = 2
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_SingleKeyValuePairMatches_ReturnsSuccess()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42,
            ["key2"] = 100
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_MultipleKeyValuePairsAllMatch_ReturnsSuccess()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<string>>
        {
            ["name"] = ValuePattern.Exact("Alice"),
            ["city"] = ValuePattern.Exact("Seattle")
        });

        var result = matcher.Evaluate(new Dictionary<string, string>
        {
            ["name"] = "Alice",
            ["city"] = "Seattle",
            ["country"] = "USA"
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_AllKeysMatchExactDictionary_ReturnsSuccess()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<int, IPattern<string>>
        {
            [1] = ValuePattern.Exact("one"),
            [2] = ValuePattern.Exact("two"),
            [3] = ValuePattern.Exact("three")
        });

        var result = matcher.Evaluate(new Dictionary<int, string>
        {
            [1] = "one",
            [2] = "two",
            [3] = "three"
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_KeyNotFoundInDictionary_ReturnsFailure()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["missing"] = ValuePattern.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 1,
            ["key2"] = 2
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Key 'missing' not found in dictionary", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_ValueDoesNotMatch_ReturnsFailure()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 100
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Expected 42, got 100", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_MultipleKeysNotFound_ReturnsFailureWithAllReasons()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["missing1"] = ValuePattern.Exact(1),
            ["missing2"] = ValuePattern.Exact(2)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 100
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains("Key 'missing1' not found"));
        Assert.Contains(failure.Reasons, r => r.Contains("Key 'missing2' not found"));
    }

    [Fact]
    public void EvaluateMatchAll_MultipleValueMismatches_ReturnsFailureWithAllReasons()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42),
            ["key2"] = ValuePattern.Exact(100)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 1,
            ["key2"] = 2
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains("Expected 42, got 1"));
        Assert.Contains(failure.Reasons, r => r.Contains("Expected 100, got 2"));
    }

    [Fact]
    public void EvaluateMatchAll_MixedFailures_ReturnsFailureWithAllReasons()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42),
            ["missing"] = ValuePattern.Exact(100)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 1
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains("Expected 42, got 1"));
        Assert.Contains(failure.Reasons, r => r.Contains("Key 'missing' not found"));
    }

    [Fact]
    public void EvaluateMatchAll_EmptyDictionaryWithPatterns_ReturnsFailure()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>());

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Key 'key1' not found", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_PathIsIncludedInFailureMessages()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["key1"] = ValuePattern.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 100
        }, ".config");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".config[key1]:", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_ComplexNestedValues_WorksCorrectly()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<string[]>>
        {
            ["tags"] = new CollectionPattern<string>.MatchAll([
                ValuePattern.Exact("tag1"),
                ValuePattern.Exact("tag2")
            ])
        });

        var result = matcher.Evaluate(new Dictionary<string, string[]>
        {
            ["tags"] = ["tag1", "tag2", "tag3"]
        });

        Assert.IsType<MatchResult.Success>(result);
    }
}