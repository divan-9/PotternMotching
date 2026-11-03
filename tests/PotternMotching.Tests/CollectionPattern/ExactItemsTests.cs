namespace PotternMotching.Tests.CollectionPattern;

using PotternMotching.Patterns;
using Xunit;

public class ExactItemsTests
{
    [Fact]
    public void EvaluateExactItems_EmptyPatternsMatchEmptyCollection_ReturnsSuccess()
    {
        var matcher = CollectionPattern.ExactItems<int>([]);

        var result = matcher.Evaluate([]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactItems_EmptyPatternsDoNotMatchNonEmptyCollection_ReturnsFailure()
    {
        var matcher = CollectionPattern.ExactItems<int>([]);

        var result = matcher.Evaluate([1], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Expected 0 item(s), got 1", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactItems_MultiplePatternsMatchInAnyOrder_ReturnsSuccess()
    {
        var matcher = CollectionPattern.ExactItems([
            ValuePattern.Exact(1),
            ValuePattern.Exact(3),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactItems_ValueArray_UsesExactValueMatching()
    {
        var matcher = CollectionPattern.ExactItems(["admin", "editor"]);

        var result = matcher.Evaluate(["editor", "admin"]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactItems_OverlappingPatterns_UsesNonGreedyAssignment_ReturnsSuccess()
    {
        var matcher = CollectionPattern.ExactItems<int>([
            ValuePattern.Gt(0),
            ValuePattern.Exact(1)
        ]);

        var result = matcher.Evaluate([1, 2]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactItems_DuplicateValues_RequireDistinctItems_ReturnsSuccess()
    {
        var matcher = CollectionPattern.ExactItems([1, 1]);

        var result = matcher.Evaluate([1, 1]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExactItems_ExtraItem_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionPattern.ExactItems([1, 2]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.ExactItems]", failure.Reasons[0]);
        Assert.Contains("Expected 2 item(s), got 3", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactItems_MissingItem_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionPattern.ExactItems([1, 2]);

        var result = matcher.Evaluate([1], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Expected 2 item(s), got 1", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExactItems_PatternNotMatched_ReturnsFailureWithPatternDetails()
    {
        var matcher = CollectionPattern.ExactItems([
            ValuePattern.Exact(1),
            ValuePattern.Exact(1)
        ]);

        var result = matcher.Evaluate([1, 2], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.ExactItems]", failure.Reasons[0]);
        Assert.Contains("Could not match pattern[1]", failure.Reasons[0]);
        Assert.Contains("unused item", failure.Reasons[0]);
    }
}
