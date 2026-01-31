namespace PotternMotching.Tests;

using Xunit;

public class MatchResultTests
{
    [Fact]
    public void Success_Create_CreatesSuccessInstance()
    {
        var result = new MatchResult.Success();

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void Success_ToString_ReturnsSuccess()
    {
        var result = new MatchResult.Success();

        Assert.Equal("Success", result.ToString());
    }

    [Fact]
    public void Failure_CreateWithSingleReason_CreatesFailureInstance()
    {
        var result = new MatchResult.Failure(["Test reason"]);

        Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(result.Reasons);
        Assert.Equal("Test reason", result.Reasons[0]);
    }

    [Fact]
    public void Failure_CreateWithMultipleReasons_CreatesFailureInstance()
    {
        var result = new MatchResult.Failure(["Reason 1", "Reason 2", "Reason 3"]);

        Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(3, result.Reasons.Length);
        Assert.Equal("Reason 1", result.Reasons[0]);
        Assert.Equal("Reason 2", result.Reasons[1]);
        Assert.Equal("Reason 3", result.Reasons[2]);
    }

    [Fact]
    public void Failure_ToStringWithNoReasons_ReturnsFailureNoReasonsProvided()
    {
        var result = new MatchResult.Failure([]);

        Assert.Equal("Failure: No reasons provided", result.ToString());
    }

    [Fact]
    public void Failure_ToStringWithSingleReason_ReturnsOneLineFormat()
    {
        var result = new MatchResult.Failure(["Single reason"]);

        Assert.Equal("Failure: Single reason", result.ToString());
    }

    [Fact]
    public void Failure_ToStringWithMultipleReasons_ReturnsBulletedListFormat()
    {
        var result = new MatchResult.Failure(["Reason 1", "Reason 2"]);

        var expected = $"Failure:{Environment.NewLine}  - Reason 1{Environment.NewLine}  - Reason 2";
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void Combine_AllSuccesses_ReturnsSuccess()
    {
        var results = new MatchResult[]
        {
            new MatchResult.Success(),
            new MatchResult.Success(),
            new MatchResult.Success()
        };

        var combined = MatchResult.Combine(results);

        Assert.IsType<MatchResult.Success>(combined);
    }

    [Fact]
    public void Combine_WithOneFailure_ReturnsFailureWithReasons()
    {
        var results = new MatchResult[]
        {
            new MatchResult.Success(),
            new MatchResult.Failure(["Failure reason"]),
            new MatchResult.Success()
        };

        var combined = MatchResult.Combine(results);

        var failure = Assert.IsType<MatchResult.Failure>(combined);
        Assert.Single(failure.Reasons);
        Assert.Equal("Failure reason", failure.Reasons[0]);
    }

    [Fact]
    public void Combine_MultipleFailures_ReturnsFailureWithAllReasonsCombined()
    {
        var results = new MatchResult[]
        {
            new MatchResult.Failure(["Reason 1", "Reason 2"]),
            new MatchResult.Success(),
            new MatchResult.Failure(["Reason 3"]),
            new MatchResult.Failure(["Reason 4", "Reason 5"])
        };

        var combined = MatchResult.Combine(results);

        var failure = Assert.IsType<MatchResult.Failure>(combined);
        Assert.Equal(5, failure.Reasons.Length);
        Assert.Equal("Reason 1", failure.Reasons[0]);
        Assert.Equal("Reason 2", failure.Reasons[1]);
        Assert.Equal("Reason 3", failure.Reasons[2]);
        Assert.Equal("Reason 4", failure.Reasons[3]);
        Assert.Equal("Reason 5", failure.Reasons[4]);
    }

    [Fact]
    public void Combine_EmptyList_ReturnsSuccess()
    {
        var results = Array.Empty<MatchResult>();

        var combined = MatchResult.Combine(results);

        Assert.IsType<MatchResult.Success>(combined);
    }
}
