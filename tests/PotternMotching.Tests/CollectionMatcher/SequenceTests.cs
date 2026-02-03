namespace PotternMotching.Tests.CollectionPattern;

using PotternMotching.Patterns;
using Xunit;

public class SequenceTests
{
    [Fact]
    public void EvaluateSequence_EmptySequenceMatchesEmptyCollection_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Sequence<int>([]);

        var result = matcher.Evaluate([]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateSequence_SingleItemSequence_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Sequence([ValuePattern.Exact(42)]);

        var result = matcher.Evaluate([42]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateSequence_MultipleItemsInExactOrder_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateSequence_NestedMatchers_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact("a"),
            ValuePattern.Exact("b"),
            ValuePattern.Exact("c")
        ]);

        var result = matcher.Evaluate(["a", "b", "c"]);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateSequence_CollectionTooShort_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.Sequence]", failure.Reasons[0]);
        Assert.Contains("at least 3", failure.Reasons[0]);
        Assert.Contains("got 2", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateSequence_CollectionTooLong_ReturnsFailureWithLengthMessage()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 2, 3, 4], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items", failure.Reasons[0]);
        Assert.Contains("[CollectionPattern.Sequence]", failure.Reasons[0]);
        Assert.Contains("length 2", failure.Reasons[0]);
        Assert.Contains("more than 2", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateSequence_WrongOrder_ReturnsFailureWithFirstMismatch()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 3, 2], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[1]", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
        Assert.Contains("Expected 2", failure.Reasons[0]);
        Assert.Contains("got 3", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateSequence_ItemMismatchAtIndex0_ReturnsFailureWithCorrectIndex()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact("a"),
            ValuePattern.Exact("b")
        ]);

        var result = matcher.Evaluate(["x", "b"], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[0]", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateSequence_ItemMismatchAtIndex2_ReturnsFailureWithCorrectIndex()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3),
            ValuePattern.Exact(4)
        ]);

        var result = matcher.Evaluate([1, 2, 99, 4], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Items[2]", failure.Reasons[0]);
        Assert.Contains("Expected 3", failure.Reasons[0]);
        Assert.Contains("got 99", failure.Reasons[0]);
    }

    [Fact]
    public void EvaluateSequence_UseEnumeratorNotMaterializedArray_ReturnsSuccess()
    {
        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3)
        ]);

        IEnumerable<int> GetNumbers()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void EvaluateSequence_EarlyExitOnFirstMismatch_DoesNotEnumerateEntireCollection()
    {
        var enumerationCount = 0;
        IEnumerable<int> GetNumbers()
        {
            enumerationCount++;
            yield return 1;
            enumerationCount++;
            yield return 99; // Mismatch here
            enumerationCount++;
            yield return 3;
            enumerationCount++;
            yield return 4;
        }

        var matcher = CollectionPattern.Sequence([
            ValuePattern.Exact(1),
            ValuePattern.Exact(2),
            ValuePattern.Exact(3),
            ValuePattern.Exact(4)
        ]);

        var result = matcher.Evaluate(GetNumbers());

        Assert.IsType<MatchResult.Failure>(result);
        Assert.Equal(2, enumerationCount); // Only enumerated up to the mismatch
    }
}