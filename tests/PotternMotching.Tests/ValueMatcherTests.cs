namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using Xunit;

public class ValuePatternTests
{
    // Test record types for object matching
    private record Person(string Name, int Age);

    [Fact]
    public void EvaluateExact_MatchingIntegers_ReturnsSuccess()
    {
        var matcher = ValuePattern.Exact(42);

        var result = matcher.Evaluate(42);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExact_MatchingStrings_ReturnsSuccess()
    {
        var matcher = ValuePattern.Exact("hello");

        var result = matcher.Evaluate("hello");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExact_MatchingObjects_ReturnsSuccess()
    {
        var person = new Person("Alice", 30);
        var matcher = ValuePattern.Exact(person);

        var result = matcher.Evaluate(person);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExact_MatchingEqualObjects_ReturnsSuccess()
    {
        var matcher = ValuePattern.Exact(new Person("Alice", 30));

        var result = matcher.Evaluate(new Person("Alice", 30));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExact_MatchingNullValues_ReturnsSuccess()
    {
        var matcher = ValuePattern.Exact<string?>(null);

        var result = matcher.Evaluate(null);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateExact_MismatchIntegers_ReturnsFailureWithCorrectMessage()
    {
        var matcher = ValuePattern.Exact(42);

        var result = matcher.Evaluate(100, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Value", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
        Assert.Contains("42", failure.Reasons[0]);
        Assert.Contains("100", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExact_MismatchStrings_ReturnsFailureWithCorrectMessage()
    {
        var matcher = ValuePattern.Exact("expected");

        var result = matcher.Evaluate("actual", ".Name");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Name", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
        Assert.Contains("expected", failure.Reasons[0]);
        Assert.Contains("actual", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExact_MismatchObjects_ReturnsFailureWithToStringRepresentation()
    {
        var matcher = ValuePattern.Exact(new Person("Alice", 30));

        var result = matcher.Evaluate(new Person("Bob", 25), ".Person");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Person", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
        Assert.Contains("Alice", failure.Reasons[0]);
        Assert.Contains("Bob", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExact_NullVsNonNull_ReturnsFailure()
    {
        var matcher = ValuePattern.Exact<string?>(null);

        var result = matcher.Evaluate("not null", ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
    }

    [Fact]
    public void EvaluateExact_NonNullVsNull_ReturnsFailure()
    {
        var matcher = ValuePattern.Exact("not null");

        var result = matcher.Evaluate(null!, ".Value");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
    }

    [Fact]
    public void EvaluateExact_PathIncludedInErrorMessage_EmptyPath()
    {
        var matcher = ValuePattern.Exact(1);

        var result = matcher.Evaluate(2, "");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(": [ValuePattern.Exact]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateExact_PathIncludedInErrorMessage_NestedPath()
    {
        var matcher = ValuePattern.Exact(1);

        var result = matcher.Evaluate(2, ".Property.Nested");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Contains(".Property.Nested", failure.Reasons[0]);
    }
}