namespace PotternMotching.Tests.CollectionMatcher;

using PotternMotching.Matchers;
using Xunit;

public class MatchAllTests
{
    [Fact]
    public void EvaluateMatchAll_EmptyPatternsMatchAnyCollection_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.MatchAll<int>([]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_SinglePatternFoundInCollection_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.MatchAll([ValueMatcher.Exact(2)]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_MultiplePatternsAllFound_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(3),
            ValueMatcher.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_SamePatternMatchesMultipleItems_ReturnsSuccessAfterFindingFirst()
    {
        var matcher = CollectionMatcher.MatchAll([ValueMatcher.Exact(2)]);

        var result = matcher.Evaluate([2, 2, 2]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_OrderDoesNotMatter_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(3),
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateMatchAll_PatternNotFound_ReturnsFailureWithPatternDetails()
    {
        var matcher = CollectionMatcher.MatchAll([ValueMatcher.Exact(99)]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionMatcher.MatchAll]", failure.Reasons[0]);
        Assert.Contains("pattern[0]", failure.Reasons[0]);
        Assert.Contains("1 pattern(s)", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_MultiplePatternsMissing_ReturnsFailureWithAllMissingPatterns()
    {
        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(99),
            ValueMatcher.Exact(3),
            ValueMatcher.Exact(100)
        ]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionMatcher.MatchAll]", failure.Reasons[0]);
        Assert.Contains("2 pattern(s)", failure.Reasons[0]);
        Assert.Contains("pattern[1]", failure.Reasons[0]);
        Assert.Contains("pattern[3]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_EmptyCollectionWithNonEmptyPatterns_ReturnsFailure()
    {
        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2)
        ]);

        var result = matcher.Evaluate(Array.Empty<int>(), ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("2 pattern(s)", failure.Reasons[0]);
        Assert.Contains("pattern[0]", failure.Reasons[0]);
        Assert.Contains("pattern[1]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_FailedPatternShowsMatcherInMessage_VerifyFormat()
    {
        var matcher = CollectionMatcher.MatchAll([ValueMatcher.Exact("missing")]);

        var result = matcher.Evaluate(["a", "b", "c"], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        // Verify the matcher details are shown
        Assert.Contains("Exact", failure.Reasons[0]);
        Assert.Contains("missing", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateMatchAll_SinglePassVerification_CollectionOnlyEnumeratedOnce()
    {
        var enumerationCount = 0;
        IEnumerable<int> GetNumbers()
        {
            enumerationCount++;
            yield return 1;
            enumerationCount++;
            yield return 2;
            enumerationCount++;
            yield return 3;
        }

        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(3, enumerationCount);
    }

    [Fact]
    public void EvaluateMatchAll_PatternsRemovedAsMatched_EarlyExitWhenAllMatched()
    {
        var enumerationCount = 0;
        IEnumerable<int> GetNumbers()
        {
            enumerationCount++;
            yield return 1;
            enumerationCount++;
            yield return 2;
            enumerationCount++;
            yield return 3;
            enumerationCount++;
            yield return 4;
            enumerationCount++;
            yield return 5;
        }

        var matcher = CollectionMatcher.MatchAll([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(2, enumerationCount); // Should stop after finding both patterns
    }
}