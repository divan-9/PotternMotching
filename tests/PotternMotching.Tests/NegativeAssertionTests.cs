namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using Xunit;
using CollectionPatternFactory = PotternMotching.Patterns.CollectionPattern;

public class NegativeAssertionTests
{
    [Fact]
    public void AssertNotPattern_WhenInnerPatternFails_DoesNotThrow()
    {
        var role = "guest";

        role.AssertNotPattern(ValuePattern.Exact("admin"));
    }

    [Fact]
    public void AssertNotPattern_WhenInnerPatternMatches_ThrowsWithPath()
    {
        var role = "admin";

        var exception = Assert.Throws<AssertionFailedException>(() =>
            role.AssertNotPattern(ValuePattern.Exact("admin")));

        Assert.Contains("role: [Pattern.Not]", exception.Message);
    }

    [Fact]
    public void AssertNotPattern_WhenNullCollectionFailsInnerPattern_DoesNotThrow()
    {
        IEnumerable<string>? items = null;

        items.AssertNotPattern(CollectionPatternFactory.AnyElement("admin"));
    }

    [Fact]
    public void AssertNotPattern_WhenInnerPatternMatchesNull_Throws()
    {
        string? role = null;

        var exception = Assert.Throws<AssertionFailedException>(() =>
            role.AssertNotPattern(ValuePattern.Exact<string?>(null)));

        Assert.Contains("role: [Pattern.Not]", exception.Message);
    }

    [Fact]
    public void AssertNotExact_WhenValuesDiffer_DoesNotThrow()
    {
        var role = "guest";

        role.AssertNotExact("admin");
    }

    [Fact]
    public void AssertNotExact_WhenValuesEqual_ThrowsWithPath()
    {
        var role = "admin";

        var exception = Assert.Throws<AssertionFailedException>(() => role.AssertNotExact("admin"));

        Assert.Contains("role: [Pattern.Not]", exception.Message);
    }

    [Fact]
    public void AssertNotExact_WhenTargetIsNullAndExpectedIsNonNull_DoesNotThrow()
    {
        string? role = null;

        role.AssertNotExact("admin");
    }

    [Fact]
    public void AssertNotExact_WhenBothValuesAreNull_Throws()
    {
        string? role = null;

        var exception = Assert.Throws<AssertionFailedException>(() =>
            role.AssertNotExact((string?)null));

        Assert.Contains("role: [Pattern.Not]", exception.Message);
    }

    [Fact]
    public void AssertContainsNone_WhenNoForbiddenValuesOccur_DoesNotThrow()
    {
        string[] items = ["a"];

        items.AssertContainsNone(["x", "c"]);
    }

    [Fact]
    public void AssertContainsNone_WhenForbiddenValueOccurs_ThrowsWithIndex()
    {
        string[] items = ["a", "b"];

        var exception = Assert.Throws<AssertionFailedException>(() =>
            items.AssertContainsNone(["x", "b"]));

        Assert.Contains("items[1]: [AssertContainsNone]", exception.Message);
        Assert.Contains("b", exception.Message);
    }

    [Fact]
    public void AssertContainsNone_WhenNoForbiddenPatternsMatch_DoesNotThrow()
    {
        int[] items = [1, 2];

        items.AssertContainsNone([ValuePattern.Gt(5)]);
    }

    [Fact]
    public void AssertContainsNone_WhenForbiddenPatternMatches_ThrowsWithIndex()
    {
        int[] items = [1, 6];

        var exception = Assert.Throws<AssertionFailedException>(() =>
            items.AssertContainsNone([ValuePattern.Gt(5)]));

        Assert.Contains("items[1]: [AssertContainsNone]", exception.Message);
    }

    [Fact]
    public void AssertContainsNone_WhenForbiddenValuesAreEmpty_DoesNotThrow()
    {
        string[] items = ["a"];

        items.AssertContainsNone([]);
    }

    [Fact]
    public void AssertContainsNone_WhenForbiddenValueIsNull_RejectsNullItem()
    {
        string?[] items = ["allowed", null];

        var exception = Assert.Throws<AssertionFailedException>(() =>
            items.AssertContainsNone([(string?)null]));

        Assert.Contains("items[1]: [AssertContainsNone]", exception.Message);
    }

    [Fact]
    public void AssertContainsNone_StopsAtFirstMatchAndDisposesEnumerator()
    {
        var enumerations = 0;
        var disposed = false;
        var items = Enumerate();

        Assert.Throws<AssertionFailedException>(() => items.AssertContainsNone([42]));

        Assert.Equal(1, enumerations);
        Assert.True(disposed);

        IEnumerable<int> Enumerate()
        {
            enumerations++;
            try
            {
                yield return 1;
                yield return 42;
                throw new InvalidOperationException("Enumeration continued after a match.");
            }
            finally
            {
                disposed = true;
            }
        }
    }

    [Fact]
    public void AssertContainsNone_WhenCollectionIsNull_DoesNotThrow()
    {
        IEnumerable<string>? items = null;

        items.AssertContainsNone(["a"]);
    }
}
