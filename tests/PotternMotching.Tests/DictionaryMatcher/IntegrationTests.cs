namespace PotternMotching.Tests.DictionaryMatcher;

using PotternMotching.Patterns;
using Xunit;

public class IntegrationTests
{
    [Fact]
    public void DictionaryMatcher_WithNestedCollections_WorksCorrectly()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<int[]>>
        {
            ["scores"] = CollectionPattern.Sequence([
                ValuePattern.Exact(95),
                ValuePattern.Exact(87),
                ValuePattern.Exact(92)
            ])
        });

        var result = matcher.Evaluate(new Dictionary<string, int[]>
        {
            ["scores"] = [95, 87, 92],
            ["extra"] = [1, 2, 3]
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void DictionaryMatcher_WithDictionaryOfDictionaries_WorksCorrectly()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<Dictionary<string, int>>>
        {
            ["user1"] = DictionaryPattern.ExactItems(new Dictionary<string, IPattern<int>>
            {
                ["age"] = ValuePattern.Exact(30),
                ["score"] = ValuePattern.Exact(100)
            })
        });

        var result = matcher.Evaluate(new Dictionary<string, Dictionary<string, int>>
        {
            ["user1"] = new Dictionary<string, int>
            {
                ["age"] = 30,
                ["score"] = 100
            },
            ["user2"] = new Dictionary<string, int>
            {
                ["age"] = 25
            }
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExactKeys_WithNestedDictionary_FailsOnExtraNestedKeys()
    {
        var matcher = DictionaryPattern.ExactItems(new Dictionary<string, IPattern<Dictionary<string, int>>>
        {
            ["config"] = DictionaryPattern.ExactItems(new Dictionary<string, IPattern<int>>
            {
                ["timeout"] = ValuePattern.Exact(30)
            })
        });

        var result = matcher.Evaluate(new Dictionary<string, Dictionary<string, int>>
        {
            ["config"] = new Dictionary<string, int>
            {
                ["timeout"] = 30,
                ["retries"] = 3  // This extra key in nested dict should cause failure
            }
        });

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(failure.Reasons, r => r.Contains("Unexpected keys:") && r.Contains("'retries'"));
    }

    [Fact]
    public void DictionaryMatcher_FailureMessagesIncludeFullPath()
    {
        var matcher = DictionaryPattern.Items(new Dictionary<string, IPattern<Dictionary<string, int>>>
        {
            ["settings"] = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
            {
                ["value"] = ValuePattern.Exact(42)
            })
        });

        var result = matcher.Evaluate(new Dictionary<string, Dictionary<string, int>>
        {
            ["settings"] = new Dictionary<string, int>
            {
                ["value"] = 999
            }
        }, ".root");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".root[settings][value]:", failure.Reasons[0]);
        Assert.Contains("Expected 42, got 999", failure.Reasons[0]);
    }

    [Fact]
    public void DictionaryMatcher_CanDistinguishBetweenMatchAllAndExactKeys()
    {
        var testDict = new Dictionary<string, int>
        {
            ["required"] = 42,
            ["extra"] = 100
        };

        var matchAllResult = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
        {
            ["required"] = ValuePattern.Exact(42)
        }).Evaluate(testDict);

        var exactKeysResult = DictionaryPattern.ExactItems(new Dictionary<string, IPattern<int>>
        {
            ["required"] = ValuePattern.Exact(42)
        }).Evaluate(testDict);

        Assert.IsType<MatchResult.Success>(matchAllResult);
        Assert.IsType<MatchResult.Failure>(exactKeysResult);
    }
}
