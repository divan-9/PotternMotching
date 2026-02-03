namespace PotternMotching.Tests;

using Xunit;
using CM = PotternMotching.Patterns.CollectionPattern;
using VM = PotternMotching.Patterns.ValuePattern;

public class IntegrationTests
{
    // Test record types for complex scenarios
    private record Person(string Name, int Age, string[] Hobbies);
    private record Address(string Street, string City, int ZipCode);
    private record Company(string Name, Address[] Offices);

    [Fact]
    public void NestedPatterns_ValuePatternsInCollectionPatterns_ReturnsSuccess()
    {
        var matcher = CM.Sequence([
            VM.Exact(1),
            VM.Exact(2),
            VM.Exact(3)
        ]);

        var result = matcher.Evaluate([1, 2, 3], ".Numbers");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NestedPatterns_CollectionOfCollections_ReturnsSuccess()
    {
        var innerMatcher1 = CM.Sequence([
            VM.Exact(1),
            VM.Exact(2)
        ]);
        var innerMatcher2 = CM.Sequence([
            VM.Exact(3),
            VM.Exact(4)
        ]);

        var outerMatcher = CM.Sequence([
            innerMatcher1,
            innerMatcher2
        ]);

        var result = outerMatcher.Evaluate([
            [1, 2],
            [3, 4]
        ], ".Matrix");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void NestedPatterns_ComplexObjectMatching_SimilarToSampleProgram()
    {
        var person = new Person("Alice", 30, ["Reading", "Coding"]);

        var hobbyMatcher = CM.Subset([
            VM.Exact("Reading"),
            VM.Exact("Coding")
        ]);

        var result = hobbyMatcher.Evaluate(person.Hobbies, ".Person.Hobbies");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ErrorMessageQuality_FullPathInNestedScenarios_ShowsCompletePath()
    {
        var innerMatcher = CM.Sequence([
            VM.Exact(1),
            VM.Exact(99) // Will fail
        ]);

        var outerMatcher = CM.Sequence<IEnumerable<int>>([
            innerMatcher
        ]);

        var result = outerMatcher.Evaluate([
            [1, 2]
        ], ".Property");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Property[0][1]", failure.Reasons[0]);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
    }

    [Fact]
    public void ErrorMessageQuality_MatcherTypesShown_IndicatesWhichMatcherFailed()
    {
        var matcher = CM.StartsWith([
            VM.Exact(1),
            VM.Exact(2)
        ]);

        var result = matcher.Evaluate([1, 99], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("[ValuePattern.Exact]", failure.Reasons[0]);
    }

    [Fact]
    public void ErrorMessageQuality_ReadableExpectedVsActual_ShowsBothValues()
    {
        var matcher = VM.Exact(new Person("Alice", 30, []));

        var result = matcher.Evaluate(new Person("Bob", 25, []), ".Person");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains("Alice", failure.Reasons[0]);
        Assert.Contains("Bob", failure.Reasons[0]);
        Assert.Contains("30", failure.Reasons[0]);
        Assert.Contains("25", failure.Reasons[0]);
    }

    [Fact]
    public void MultipleFailures_CombineCollectsAll_ShowsAllReasons()
    {
        var result1 = new MatchResult.Failure([".Path1: Error 1"]);
        var result2 = new MatchResult.Failure([".Path2: Error 2"]);
        var result3 = new MatchResult.Failure([".Path3: Error 3"]);

        var combined = MatchResult.Combine([result1, result2, result3]);

        var failure = Assert.IsType<MatchResult.Failure>(combined);
        Assert.Equal(3, failure.Reasons.Length);
        Assert.Contains(".Path1: Error 1", failure.Reasons);
        Assert.Contains(".Path2: Error 2", failure.Reasons);
        Assert.Contains(".Path3: Error 3", failure.Reasons);
    }

    [Fact]
    public void MultipleFailures_FailFastMatchers_ReportFirstFailure()
    {
        // Sequence matcher stops at first failure
        var matcher = CM.Sequence([
            VM.Exact(1),
            VM.Exact(99), // First failure
            VM.Exact(100) // Would also fail but won't be checked
        ]);

        var result = matcher.Evaluate([1, 2, 3], ".Items");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons); // Only first failure
        Assert.Contains(".Items[1]", failure.Reasons[0]);
        Assert.Contains("Expected 99", failure.Reasons[0]);
    }

    [Fact]
    public void RealWorldScenario_CompanyWithOffices_NestedObjectMatching()
    {
        var company = new Company(
            "TechCorp",
            [
                new Address("123 Main St", "San Francisco", 94102),
                new Address("456 Oak Ave", "New York", 10001)
            ]
        );

        var officeMatcher = CM.Subset([
            VM.Exact(new Address("123 Main St", "San Francisco", 94102)),
            VM.Exact(new Address("456 Oak Ave", "New York", 10001))
        ]);

        var result = officeMatcher.Evaluate(company.Offices, ".Company.Offices");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void RealWorldScenario_PartialMatch_OnlyMatchesRelevantFields()
    {
        // In real usage, you'd have custom matchers for partial matching
        // This tests that matchers compose well
        var addresses = new[]
        {
            new Address("123 Main St", "San Francisco", 94102),
            new Address("456 Oak Ave", "New York", 10001),
            new Address("789 Pine Rd", "Seattle", 98101)
        };

        var cityMatcher = CM.EndsWith([
            VM.Exact(new Address("789 Pine Rd", "Seattle", 98101))
        ]);

        var result = cityMatcher.Evaluate(addresses, ".Addresses");

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void RealWorldScenario_ComplexNestedFailure_ShowsFullPath()
    {
        var company = new Company(
            "TechCorp",
            [
                new Address("123 Main St", "San Francisco", 94102),
                new Address("WRONG", "New York", 10001)
            ]
        );

        var officeMatcher = CM.Sequence([
            VM.Exact(new Address("123 Main St", "San Francisco", 94102)),
            VM.Exact(new Address("456 Oak Ave", "New York", 10001))
        ]);

        var result = officeMatcher.Evaluate(company.Offices, ".Company.Offices");

        var failure = Assert.IsType<MatchResult.Failure>(result);
        Assert.Single(failure.Reasons);
        Assert.Contains(".Company.Offices[1]", failure.Reasons[0]);
        Assert.Contains("456 Oak Ave", failure.Reasons[0]);
        Assert.Contains("WRONG", failure.Reasons[0]);
    }

    [Fact]
    public void RealWorldScenario_MixedMatchersAndCombine_WorksTogether()
    {
        var person = new Person("Alice", 30, ["Reading", "Coding", "Gaming"]);

        var nameMatcher = VM.Exact("Alice");
        var ageMatcher = VM.Exact(30);
        var hobbyMatcher = CM.Subset([
            VM.Exact("Reading"),
            VM.Exact("Coding")
        ]);

        var nameResult = nameMatcher.Evaluate(person.Name, ".Name");
        var ageResult = ageMatcher.Evaluate(person.Age, ".Age");
        var hobbyResult = hobbyMatcher.Evaluate(person.Hobbies, ".Hobbies");

        var combined = MatchResult.Combine([nameResult, ageResult, hobbyResult]);

        Assert.IsType<MatchResult.Success>(combined);
    }

    [Fact]
    public void RealWorldScenario_MultipleMismatchesInCombine_ShowsAllErrors()
    {
        var person = new Person("Alice", 30, ["Reading", "Coding"]);

        var nameMatcher = VM.Exact("Bob"); // Will fail
        var ageMatcher = VM.Exact(25); // Will fail
        var hobbyMatcher = CM.Subset([
            VM.Exact("Swimming") // Will fail
        ]);

        var nameResult = nameMatcher.Evaluate(person.Name, ".Name");
        var ageResult = ageMatcher.Evaluate(person.Age, ".Age");
        var hobbyResult = hobbyMatcher.Evaluate(person.Hobbies, ".Hobbies");

        var combined = MatchResult.Combine([nameResult, ageResult, hobbyResult]);

        var failure = Assert.IsType<MatchResult.Failure>(combined);
        Assert.Equal(3, failure.Reasons.Length);
        Assert.Contains(failure.Reasons, r => r.Contains(".Name"));
        Assert.Contains(failure.Reasons, r => r.Contains(".Age"));
        Assert.Contains(failure.Reasons, r => r.Contains(".Hobbies"));
    }
}