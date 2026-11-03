namespace PotternMotching.Tests;

using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorAccessibilityTests
{
    [Fact]
    public void InternalRecord_GeneratesInternalPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace AccessibilityPatternTests;

            internal record Thing(string Id);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            internal static class Usage
            {
                public static ThingPattern Create() => new(Id: "42");
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var source = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
        Assert.Contains("internal sealed record ThingPattern", source.SourceText.ToString());
    }

    [Fact]
    public void InternalClass_GeneratesInternalPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace AccessibilityPatternTests;

            internal class Thing
            {
                public string Id { get; init; } = string.Empty;
            }

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            internal static class Usage
            {
                public static ThingPattern Create() => new(Id: "42");
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var source = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
        Assert.Contains("internal sealed record ThingPattern", source.SourceText.ToString());
    }

    [Fact]
    public void PublicRecord_GeneratesPublicPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace AccessibilityPatternTests;

            public record Thing(string Id);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var source = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
        Assert.Contains("public sealed record ThingPattern", source.SourceText.ToString());
    }

    [Fact]
    public void PrivateNestedRecord_ReportsUnsupportedTargetDiagnostic()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace AccessibilityPatternTests;

            public static class Container
            {
                private record Secret(string Id);

                [AutoPatternFor(typeof(Secret))]
                internal static class PatternMarkers;
            }
            """);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics.Where(d => d.Id == "PM0010"));
        Assert.Contains("Secret", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedSources, source => source.HintName.Contains("SecretPattern"));
    }
}
