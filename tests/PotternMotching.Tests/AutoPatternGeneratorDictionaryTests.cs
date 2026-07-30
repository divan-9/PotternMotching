namespace PotternMotching.Tests;

using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorDictionaryTests
{
    [Fact]
    public void IReadOnlyDictionaryProperty_GeneratesReadOnlyDictionaryPatternDefault()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using System.Collections.Generic;
            using PotternMotching;
            using PotternMotching.Patterns;

            namespace DictionaryPatternTests;

            public record HealthCheckResult(IReadOnlyDictionary<string, object> Data);

            [AutoPatternFor(typeof(HealthCheckResult))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate(HealthCheckResult value) =>
                    new HealthCheckResultPattern(
                        Data: DictionaryPattern.Items(new Dictionary<string, object>
                        {
                            ["status"] = "ok"
                        })).Evaluate(value);

                public static HealthCheckResultPattern Convert(HealthCheckResult value) =>
                    value;
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var generated = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("HealthCheckResultPattern"));
        Assert.Contains(
            "ReadOnlyDictionaryPatternDefault<string, object, ValuePattern<object>> Data = default",
            generated.SourceText.ToString());
    }
}
