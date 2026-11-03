namespace PotternMotching.Tests;

using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorPropertyNameTests
{
    [Fact]
    public void PositionalRecord_WithLowercasePropertyName_GeneratesCompilablePattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PropertyNamePatternTests;

            public record Thing(string id);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new ThingPattern(Id: "42").Evaluate(new Thing("42"));
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Class_WithLowercasePropertyName_GeneratesCompilablePattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PropertyNamePatternTests;

            public class Thing
            {
                public string id { get; init; } = string.Empty;
            }

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new ThingPattern(Id: "42").Evaluate(new Thing { id = "42" });
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void PositionalRecord_WithPatternParameterNameCollision_ReportsDiagnostic()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PropertyNamePatternTests;

            public record Thing(string id, string Id);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;
            """);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics.Where(d => d.Id == "PM0012"));
        Assert.Contains("Id", diagnostic.GetMessage());
        Assert.Contains("Thing", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
    }

    [Fact]
    public void Class_WithPatternParameterNameCollision_ReportsDiagnostic()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PropertyNamePatternTests;

            public class Thing
            {
                public string id { get; init; } = string.Empty;
                public string Id { get; init; } = string.Empty;
            }

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;
            """);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics.Where(d => d.Id == "PM0012"));
        Assert.Contains("Id", diagnostic.GetMessage());
        Assert.Contains("Thing", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
    }

    [Fact]
    public void PositionalRecord_WithKeywordPropertyName_GeneratesCompilablePattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PropertyNamePatternTests;

            public record Thing(string @class);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new ThingPattern(Class: "value").Evaluate(new Thing("value"));
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }
}
