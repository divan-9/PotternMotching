namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using Xunit;

public class AssertionTests
{
    [Fact]
    public void AssertSubset_WhenTargetContainsExpectedItems_DoesNotThrow()
    {
        var values = new[] { "alpha", "beta", "gamma" };

        values.AssertSubset(["alpha", "gamma"]);
    }

    [Fact]
    public void AssertSubset_WhenTargetDoesNotContainExpectedItem_ThrowsWithPath()
    {
        var values = new[] { "alpha", "beta" };

        var exception = Assert.Throws<AssertionFailedException>(() => values.AssertSubset(["gamma"]));

        Assert.Contains("values", exception.Message);
        Assert.Contains("CollectionPattern.Subset", exception.Message);
    }

    [Fact]
    public void AssertSubset_WhenUsingPatternArray_DoesNotThrow()
    {
        var values = new[] { "alpha", "beta", "gamma" };

        values.AssertSubset([
            ValuePattern.Exact("alpha"),
            ValuePattern.Exact("gamma")
        ]);
    }

    [Fact]
    public void AssertSequence_WhenTargetMatchesExpectedSequence_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertSequence([1, 2, 3]);
    }

    [Fact]
    public void AssertSequence_WhenTargetDoesNotMatchExpectedSequence_ThrowsWithPath()
    {
        var values = new[] { 1, 2, 3 };

        var exception = Assert.Throws<AssertionFailedException>(() => values.AssertSequence([1, 3]));

        Assert.Contains("values[1]", exception.Message);
        Assert.Contains("ValuePattern.Exact", exception.Message);
    }

    [Fact]
    public void AssertSequence_WhenUsingPatternArray_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertSequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);
    }

    [Fact]
    public void AssertEndsWith_WhenTargetEndsWithExpectedSequence_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertEndsWith([2, 3]);
    }

    [Fact]
    public void AssertEndsWith_WhenTargetDoesNotEndWithExpectedSequence_ThrowsWithPath()
    {
        var values = new[] { 1, 2, 3 };

        var exception = Assert.Throws<AssertionFailedException>(() => values.AssertEndsWith([1, 2]));

        Assert.Contains("values[^2]", exception.Message);
        Assert.Contains("ValuePattern.Exact", exception.Message);
    }

    [Fact]
    public void AssertEndsWith_WhenUsingPatternArray_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertEndsWith([
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);
    }

    [Fact]
    public void AssertStartsWith_WhenTargetStartsWithExpectedSequence_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertStartsWith([1, 2]);
    }

    [Fact]
    public void AssertStartsWith_WhenTargetDoesNotStartWithExpectedSequence_ThrowsWithPath()
    {
        var values = new[] { 1, 2, 3 };

        var exception = Assert.Throws<AssertionFailedException>(() => values.AssertStartsWith([2, 3]));

        Assert.Contains("values[0]", exception.Message);
        Assert.Contains("ValuePattern.Exact", exception.Message);
    }

    [Fact]
    public void AssertStartsWith_WhenUsingPatternArray_DoesNotThrow()
    {
        var values = new[] { 1, 2, 3 };

        values.AssertStartsWith([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2)
        ]);
    }
}
