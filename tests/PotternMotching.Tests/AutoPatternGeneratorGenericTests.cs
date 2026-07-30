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

    [Fact]
    public void ConstructedGenericUnionTarget_GeneratesVariantPatternsWithConcreteRootPatternName()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using Dunet;
            using PotternMotching;

            namespace GenericPatternTests;

            [Union]
            public abstract partial record Maybe<T>
            {
                public partial record Some(T Value);
                public partial record None;
            }

            [AutoPatternFor(typeof(Maybe<string>))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static Maybe_StringPattern CreateRoot() => new Maybe_StringPattern.Some(Value: "hello");

                public static MatchResult Evaluate() =>
                    new Maybe_StringPattern.Some(Value: "hello").Evaluate(new Maybe<string>.Some("hello"));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Id == "PM0010");

        var generated = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("Maybe_StringPattern"));
        Assert.Contains("public abstract partial record Maybe_StringPattern", generated.SourceText.ToString());
        Assert.Contains(": Maybe_StringPattern, IPattern<", generated.SourceText.ToString());
        Assert.DoesNotContain(": MaybePattern, IPattern<", generated.SourceText.ToString());
    }
}
