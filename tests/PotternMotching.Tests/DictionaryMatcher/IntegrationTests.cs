namespace PotternMotching.Tests.DictionaryMatcher;

using PotternMotching.Matchers;
using Xunit;

public class IntegrationTests
{
    [Fact]
    public void DictionaryMatcher_WithNestedCollections_WorksCorrectly()
    {
        var matcher = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int[]>>
        {
            ["scores"] = new CollectionMatcher<int>.Sequence([
                ValueMatcher.Exact(95),
                ValueMatcher.Exact(87),
                ValueMatcher.Exact(92)
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
        var matcher = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<Dictionary<string, int>>>
        {
            ["user1"] = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
            {
                ["age"] = ValueMatcher.Exact(30),
                ["score"] = ValueMatcher.Exact(100)
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
        var matcher = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<Dictionary<string, int>>>
        {
            ["config"] = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
            {
                ["timeout"] = ValueMatcher.Exact(30)
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
        Assert.Contains(failure.Reasons, r => r.Contains("Unexpected key 'retries'"));
    }

    [Fact]
    public void DictionaryMatcher_FailureMessagesIncludeFullPath()
    {
        var matcher = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<Dictionary<string, int>>>
        {
            ["settings"] = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int>>
            {
                ["value"] = ValueMatcher.Exact(42)
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

        var matchAllResult = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int>>
        {
            ["required"] = ValueMatcher.Exact(42)
        }).Evaluate(testDict);

        var exactKeysResult = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
        {
            ["required"] = ValueMatcher.Exact(42)
        }).Evaluate(testDict);

        Assert.IsType<MatchResult.Success>(matchAllResult);
        Assert.IsType<MatchResult.Failure>(exactKeysResult);
    }
}
