namespace PotternMotching.Tests.CollectionPattern;

using PotternMotching.Patterns;
using Xunit;

public class SubsetTests
{
    [Fact]
    public void EvaluateAnyOrder_EmptyPatternsMatchAnyCollection_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Subset<int>([]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateAnyOrder_SinglePatternFoundInCollection_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Subset([ValuePattern.Exact(2)]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateAnyOrder_MultiplePatternsAllFound_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(1),
            ValuePattern.Exact(3),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateAnyOrder_SamePatternMatchesMultipleItems_ReturnsSuccessAfterFindingFirst()
    {
        var matcher = CollectionPattern.Subset([ValuePattern.Exact(2)]);

        var result = matcher.Evaluate([2, 2, 2]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateAnyOrder_OrderDoesNotMatter_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(3),
            ValuePattern.Exact(1),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateAnyOrder_PatternNotFound_ReturnsFailureWithPatternDetails()
    {
        var matcher = CollectionPattern.Subset([ValuePattern.Exact(99)]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.Subset]", failure.Reasons[0]);
        Assert.Contains("pattern[0]", failure.Reasons[0]);
        Assert.Contains("1 pattern(s)", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateAnyOrder_MultiplePatternsMissing_ReturnsFailureWithAllMissingPatterns()
    {
        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(1),
            ValuePattern.Exact(99),
            ValuePattern.Exact(3),
            ValuePattern.Exact(100)
        ]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.Subset]", failure.Reasons[0]);
        Assert.Contains("2 pattern(s)", failure.Reasons[0]);
        Assert.Contains("pattern[1]", failure.Reasons[0]);
        Assert.Contains("pattern[3]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateAnyOrder_EmptyCollectionWithNonEmptyPatterns_ReturnsFailure()
    {
        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate(Array.Empty<int>(), ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("2 pattern(s)", failure.Reasons[0]);
        Assert.Contains("pattern[0]", failure.Reasons[0]);
        Assert.Contains("pattern[1]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateAnyOrder_FailedPatternShowsMatcherInMessage_VerifyFormat()
    {
        var matcher = CollectionPattern.Subset([ValuePattern.Exact("missing")]);

        var result = matcher.Evaluate(["a", "b", "c"], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        // Verify the matcher details are shown
        Assert.Contains("Exact", failure.Reasons[0]);
        Assert.Contains("missing", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateAnyOrder_SinglePassVerification_CollectionOnlyEnumeratedOnce()
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

        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(3, enumerationCount);
    }

    [Fact]
    public void EvaluateAnyOrder_PatternsRemovedAsMatched_EarlyExitWhenAllMatched()
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

        var matcher = CollectionPattern.Subset([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(2, enumerationCount); // Should stop after finding both patterns
    }
}