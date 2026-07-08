namespace PotternMotching.Tests;

using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorGenericTests
{
    [Fact]
    public void OpenGenericTarget_ReportsUnsupportedExternalTargetDiagnostic()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace GenericPatternTests;

            public record GenericBox<T>(T Value);

            [AutoPatternFor(typeof(GenericBox<>))]
            internal static class PatternMarkers;
            """);

        var diagnostic = Assert.Single(result.GeneratorDiagnostics.Where(d => d.Id == "PM0010"));
        Assert.Contains("GenericBox", diagnostic.GetMessage());
        Assert.DoesNotContain(result.GeneratedSources, source => source.HintName.Contains("GenericBoxPattern"));
    }

    [Fact]
    public void ConstructedGenericTarget_GeneratesPatternWithConcreteTypeSuffix()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace GenericPatternTests;

            public record GenericBox<T>(T Value);

            [AutoPatternFor(typeof(GenericBox<string>))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static GenericBox_StringPattern Create() => new(Value: "hello");
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Id == "PM0010");
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Contains(result.GeneratedSources, source => source.HintName.Contains("GenericBox_StringPattern"));
    }
}
