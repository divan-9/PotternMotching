namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using Xunit;
using CollectionPatternFactory = PotternMotching.Patterns.CollectionPattern;

public class NotPatternTests
{
    [Fact]
    public void Evaluate_WhenInnerPatternFails_ReturnsSuccess()
    {
        var pattern = Pattern.Not(ValuePattern.Exact("admin"));

        var result = pattern.Evaluate("guest", ".Role");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void Evaluate_WhenInnerPatternMatches_ReturnsFailureWithPathAndPattern()
    {
        var pattern = Pattern.Not(ValuePattern.Exact("admin"));

        var result = pattern.Evaluate("admin", ".Role");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        var reason = Assert.Single(failure.Reasons);
        Assert.Contains(".Role: [Pattern.Not]", reason);
        Assert.Contains("admin", reason);
    }

    [Fact]
    public void Evaluate_WhenCollectionPatternRejectsNull_ReturnsSuccess()
    {
        var pattern = Pattern.Not(CollectionPatternFactory.Sequence(["a"]));

        var result = pattern.Evaluate(null!, ".Items");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void Evaluate_WhenNegatingExactNull_ReturnsFailureForNull()
    {
        var pattern = Pattern.Not(ValuePattern.Exact<string[]?>(null));

        var result = pattern.Evaluate(null, ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Items", Assert.Single(failure.Reasons));
    }

    [Fact]
    public void Evaluate_WhenNegatingExactNull_ReturnsSuccessForEmptyCollection()
    {
        var pattern = Pattern.Not(ValuePattern.Exact<string[]?>(null));

        var result = pattern.Evaluate([], ".Items");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void Evaluate_WhenInnerPatternThrows_PropagatesException()
    {
        var pattern = Pattern.Not(new ThrowingPattern());

        Assert.Throws<InvalidOperationException>(() => pattern.Evaluate(1));
    }

    private sealed class ThrowingPattern : IPattern<int>
    {
        public MatchResult Evaluate(int value, string path = "") => throw new InvalidOperationException();
    }
}
