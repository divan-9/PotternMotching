namespace PotternMotching.Tests;

using PotternMotching.Patterns;
using PotternMotching.TestExternalModels;
using PotternMotching.Tests.ExternalPatterns;
using Xunit;

public class ExternalAutoPatternTests
{
    [Fact]
    public void ExternalRecord_GeneratesPatternAndMatches()
    {
        var user = new ExternalUserDto(
            Id: "42",
            Name: "Alice",
            Roles: ["admin", "editor"],
            Address: new ExternalAddress("Seattle", "98101"));

        var pattern = new ExternalUserDtoPattern(
            Id: "42",
            Address: new ExternalAddressPattern(City: "Seattle"));

        var result = pattern.Evaluate(user);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_CollectionsUseExistingWrapperRules()
    {
        var dto = new ExternalCollectionsDto(
            Roles: ["admin", "editor"],
            Numbers: [1, 2, 3],
            Flags: ["beta", "dark-mode"],
            Scores: new Dictionary<string, int>
            {
                ["quality"] = 10,
                ["speed"] = 8,
            });

        var pattern = new ExternalCollectionsDtoPattern(
            Roles: ["admin", "editor"],
            Numbers: [1, 2, 3],
            Flags: ["beta"],
            Scores: DictionaryPattern.Items(new Dictionary<string, int>
            {
                ["quality"] = 10,
            }));

        var result = pattern.Evaluate(dto);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_ImplicitConversionToPattern_Works()
    {
        ExternalAddressPattern pattern = new ExternalAddress("Seattle", "98101");

        var result = pattern.Evaluate(new ExternalAddress("Seattle", "98101"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalRecord_NestedUnknownRecord_FallsBackToValueMatching()
    {
        var value = new ExternalWrappedUnknown(
            Id: "42",
            Unknown: new ExternalUnknown("hello"));

        var pattern = new ExternalWrappedUnknownPattern(
            Unknown: new ExternalUnknown("hello"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClass_PublicReadableProperties_AreGeneratedAndMatched()
    {
        var value = new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
            Hidden = "secret",
        };

        var pattern = new ExternalClassDtoPattern(
            Id: "42",
            Name: "Alice",
            RoleCount: 2,
            Address: new ExternalAddressPattern(City: "Seattle"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalClass_ImplicitConversionToPattern_Works()
    {
        ExternalClassDtoPattern pattern = new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
        };

        var result = pattern.Evaluate(new ExternalClassDto
        {
            Id = "42",
            Name = "Alice",
            Roles = ["admin", "editor"],
            Address = new ExternalAddress("Seattle", "98101"),
        });

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_RootPattern_Works()
    {
        ExternalJobPattern pattern = new ExternalJobPattern.Employed(
            Company: "Tech Corp",
            Position: "Developer");

        var result = pattern.Evaluate(new ExternalJob.Employed("Tech Corp", "Developer"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_ImplicitConversionToBasePattern_Works()
    {
        ExternalJobPattern pattern = new ExternalJob.Employed("Tech Corp", "Developer");

        var result = pattern.Evaluate(new ExternalJob.Employed("Tech Corp", "Developer"));

        Assert.IsType<MatchResult.Success>(result);
    }

    [Fact]
    public void ExternalUnion_NestedInExternalRecord_UsesGeneratedPattern()
    {
        var value = new ExternalJobApplication(
            CompanyName: "Acme Corp",
            DesiredPosition: new ExternalJob.Employed("Tech Corp", "Engineer"));

        var pattern = new ExternalJobApplicationPattern(
            CompanyName: "Acme Corp",
            DesiredPosition: new ExternalJobPattern.Employed(
                Company: "Tech Corp",
                Position: "Engineer"));

        var result = pattern.Evaluate(value);

        Assert.IsType<MatchResult.Success>(result);
    }
}
