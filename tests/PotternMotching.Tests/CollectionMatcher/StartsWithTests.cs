namespace PotternMotching.Tests.CollectionMatcher;

using PotternMotching.Matchers;
using Xunit;

public class StartsWithTests
{
    [Fact]
    public void EvaluateStartsWith_EmptyPatternMatchesAnyCollection_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.StartsWith<int>([]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateStartsWith_SingleItemAtStart_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.StartsWith([ValueMatcher.Exact(1)]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateStartsWith_MultipleItemsAtStart_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4, 5]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateStartsWith_ExactSameLength_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateStartsWith_CollectionLongerThanPattern_ReturnsSuccess()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact("a"),
            ValueMatcher.Exact("b")
        ]);

        var result = matcher.Evaluate(new[] { "a", "b", "c", "d", "e" });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateStartsWith_CollectionTooShort_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionMatcher.StartsWith]", failure.Reasons[0]);
        Assert.Contains("start with 3 items", failure.Reasons[0]);
        Assert.Contains("only 2 items", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateStartsWith_FirstItemMismatch_ReturnsFailureWithIndex0()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(99),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[0]", failure.Reasons[0]);
        Assert.Contains("[ValueMatcher.Exact]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
        Assert.Contains("got 1", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateStartsWith_LaterItemMismatch_ReturnsFailureWithCorrectIndex()
    {
        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(99)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4, 5], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[2]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
        Assert.Contains("got 3", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateStartsWith_EmptyCollectionWithNonEmptyPattern_ReturnsFailure()
    {
        var matcher = CollectionMatcher.StartsWith([ValueMatcher.Exact(1)]);

        var result = matcher.Evaluate(Array.Empty<int>(), ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("start with 1 items", failure.Reasons[0]);
        Assert.Contains("only 0 items", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateStartsWith_DoesNotEnumerateEntireCollection_StopsAfterMatchingPatterns()
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

        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(2, enumerationCount); // Should only enumerate what's needed
    }

    [Fact]
    public void EvaluateStartsWith_StopsOnFirstMismatch_DoesNotContinueEnumerating()
    {
        var enumerationCount = 0;
        IEnumerable<int> GetNumbers()
        {
            enumerationCount++;
            yield return 1;
            enumerationCount++;
            yield return 99; // Mismatch
            enumerationCount++;
            yield return 3;
            enumerationCount++;
            yield return 4;
        }

        var matcher = CollectionMatcher.StartsWith([
            ValueMatcher.Exact(1),
            ValueMatcher.Exact(2),
            ValueMatcher.Exact(3)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, enumerationCount); // Stopped at mismatch
    }
}
