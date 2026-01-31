namespace PotternMotching.Tests.DictionaryMatcher;

using PotternMotching.Matchers;
using Xunit;

public class ExactKeysTests
{
    [Fact]
    public void EvaluateExactKeys_EmptyPatternMatchesEmptyDictionary_ReturnsSuccess()
    {
        var matcher = DictionaryMatcher.ExactKeys<string, int>(new Dictionary<string, IMatcher<int>>());

        var result = matcher.Evaluate(new Dictionary<string, int>());

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactKeys_ExactMatchWithSingleKey_ReturnsSuccess()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactKeys_ExactMatchWithMultipleKeys_ReturnsSuccess()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<string>>
        {
            ["name"] = ValueMatcher.Exact("Alice"),
            ["city"] = ValueMatcher.Exact("Seattle")
        });

        var result = matcher.Evaluate(new Dictionary<string, string>
        {
            ["name"] = "Alice",
            ["city"] = "Seattle"
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactKeys_DictionaryHasExtraKey_ReturnsFailure()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42,
            ["extra"] = 100
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Unexpected key 'extra' found in dictionary", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactKeys_DictionaryMissingKey_ReturnsFailure()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42),
            ["key2"] = ValueMatcher.Exact(100)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Key 'key2' not found in dictionary", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactKeys_ValueMismatch_ReturnsFailure()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
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
    public void EvaluateExactKeys_MultipleExtraKeys_ReturnsFailureWithAllReasons()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42,
            ["extra1"] = 1,
            ["extra2"] = 2
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains("Unexpected key 'extra1'"));
        Assert.Contains(failure.Reasons, r => r.Contains("Unexpected key 'extra2'"));
    }

    [Fact]
    public void EvaluateExactKeys_MixedFailures_ReturnsFailureWithAllReasons()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42),
            ["key2"] = ValueMatcher.Exact(100)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 1,
            ["extra"] = 999
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(3, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains("Expected 42, got 1"));
        Assert.Contains(failure.Reasons, r => r.Contains("Key 'key2' not found"));
        Assert.Contains(failure.Reasons, r => r.Contains("Unexpected key 'extra'"));
    }

    [Fact]
    public void EvaluateExactKeys_EmptyPatternWithNonEmptyDictionary_ReturnsFailure()
    {
        var matcher = DictionaryMatcher.ExactKeys<string, int>(new Dictionary<string, IMatcher<int>>());

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Unexpected key 'key1' found in dictionary", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactKeys_NonEmptyPatternWithEmptyDictionary_ReturnsFailure()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>());

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Key 'key1' not found", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactKeys_PathIsIncludedInFailureMessages()
    {
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var result = matcher.Evaluate(new Dictionary<string, int>
        {
            ["key1"] = 42,
            ["extra"] = 100
        }, ".settings");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".settings[extra]:", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactKeys_ComparedToMatchAll_DifferentBehavior()
    {
        var exactKeysMatcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var matchAllMatcher = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int>>
        {
            ["key1"] = ValueMatcher.Exact(42)
        });

        var dict = new Dictionary<string, int>
        {
            ["key1"] = 42,
            ["extra"] = 100
        };

        var exactKeysResult = exactKeysMatcher.Evaluate(dict);
        var matchAllResult = matchAllMatcher.Evaluate(dict);

        // ExactKeys should fail due to extra key
        Assert.IsType<MatchResult.Failure>(exactKeysResult);

        // MatchAll should succeed (allows extra keys)
        Assert.IsType<MatchResult.Success>(matchAllResult);
    }
}
