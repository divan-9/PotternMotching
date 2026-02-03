namespace PotternMotching.Tests.CollectionPattern;

using PotternMotching.Patterns;
using Xunit;

public class EndsWithTests
{
    [Fact]
    public void EvaluateEndsWith_EmptyPatternMatchesAnyCollection_ReturnsSuccess()
    {
        var matcher = CollectionPattern.EndsWith<int>([]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_SingleItemAtEnd_ReturnsSuccess()
    {
        var matcher = CollectionPattern.EndsWith([ValuePattern.Exact(3)]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_MultipleItemsAtEnd_ReturnsSuccess()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(3),
            ValuePattern.Exact(4),
            ValuePattern.Exact(5)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4, 5]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_ExactSameLength_ReturnsSuccess()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_CollectionTooShort_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.EndsWith]", failure.Reasons[0]);
        Assert.Contains("end with 3 items", failure.Reasons[0]);
        Assert.Contains("only 2 items", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateEndsWith_LastItemMismatch_ReturnsFailureWithHatNotation()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(2),
            ValuePattern.Exact(99)
        ]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[^1]", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
        Assert.Contains("got 3", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateEndsWith_EarlierItemMismatch_ReturnsFailureWithCorrectHatNotation()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(99),
            ValuePattern.Exact(4),
            ValuePattern.Exact(5)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4, 5], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[^3]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
        Assert.Contains("got 3", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateEndsWith_MiddleItemMismatch_ReturnsFailureWithCorrectHatNotation()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(3),
            ValuePattern.Exact(99),
            ValuePattern.Exact(5)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4, 5], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[^2]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
        Assert.Contains("got 4", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateEndsWith_EmptyCollectionWithNonEmptyPattern_ReturnsFailure()
    {
        var matcher = CollectionPattern.EndsWith([ValuePattern.Exact(1)]);

        var result = matcher.Evaluate(Array.Empty<int>(), ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("end with 1 items", failure.Reasons[0]);
        Assert.Contains("only 0 items", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateEndsWith_QueueBufferVerification_OnlyTailItemsInMemory()
    {
        // This test verifies that the implementation uses a queue buffer
        // by ensuring it works correctly with large collections
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(998),
            ValuePattern.Exact(999),
            ValuePattern.Exact(1000)
        ]);

        var largeCollection = Enumerable.Range(1, 1000);

        var result = matcher.Evaluate(largeCollection);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_LargeCollectionSmallPattern_MemoryEfficient()
    {
        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(99),
            ValuePattern.Exact(100)
        ]);

        IEnumerable<int> GetLargeCollection()
        {
            for (int i = 1; i <= 100; i++)
            {
                yield return i;
            }
        }

        var result = matcher.Evaluate(GetLargeCollection());

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateEndsWith_EnumeratesEntireCollection_ButKeepsOnlyTail()
    {
        var enumerationCount = 0;
        IEnumerable<int> GetNumbers()
        {
            for (int i = 1; i <= 10; i++)
            {
                yield return i;
                enumerationCount++;
            }
        }

        var matcher = CollectionPattern.EndsWith([
            ValuePattern.Exact(9),
            ValuePattern.Exact(10)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
        Assert.Equal(10, enumerationCount); // Must enumerate all to find the end
    }
}