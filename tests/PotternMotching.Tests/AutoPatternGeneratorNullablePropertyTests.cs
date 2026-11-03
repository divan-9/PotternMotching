namespace PotternMotching.Tests;

using Microsoft.CodeAnalysis;
using PotternMotching.SourceGen;
using Xunit;

public class AutoPatternGeneratorNullablePropertyTests
{
    [Fact]
    public void NullableScalarProperty_AcceptsNullLiteralInGeneratedPatternConstructor()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace NullablePatternTests;

            public record Thing(string? RuleSetId);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ThingPattern Create() => new(RuleSetId: (string?)null);
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NullableScalarProperty_OnClosedGenericTarget_AcceptsNullLiteralInGeneratedPatternConstructor()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace NullablePatternTests;

            public interface IContext;
            public interface ICondition<TContext> where TContext : IContext;
            public sealed record AnyContext() : IContext;
            public sealed record AnyCondition() : ICondition<AnyContext>;

            public record ImpressionRuleV2<TContext, TCondition>(
                string Id,
                TCondition Condition,
                string? RuleSetId)
                where TContext : IContext
                where TCondition : ICondition<TContext>;

            [AutoPatternFor(typeof(ImpressionRuleV2<AnyContext, AnyCondition>))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ImpressionRuleV2_AnyContext_AnyConditionPattern Create() =>
                    new(Id: "42", Condition: new AnyCondition(), RuleSetId: (string?)null);
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NullableNestedProperty_UsesNullablePatternDefault()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;
            using PotternMotching.Patterns;

            namespace NullablePatternTests;

            public record Options(long Width, long Height);
            public record Thing(string Id, Options? Options);

            [AutoPatternFor(typeof(Options))]
            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ThingPattern MatchNestedShape() =>
                    new(Options: new OptionsPattern(Width: 100));

                public static ThingPattern MatchNull() =>
                    new(Options: ValuePattern.Null());

                public static ThingPattern MatchNullableValue() =>
                    new(Options: (Options?)null);

                public static ThingPattern MatchExactNullablePattern() =>
                    new(Options: ValuePattern.Exact<Options?>(null));
            }
            """);

        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Id == "CS8631");

        var generated = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("ThingPattern"));
        Assert.Contains(
            "NullablePatternDefault<global::NullablePatternTests.Options, global::NullablePatternTests.OptionsPattern> Options = default",
            generated.SourceText.ToString());
    }

    [Fact]
    public void NullLiteralAnalyzer_WarnsForExplicitNullableCast_AndSuggestsValuePatternNull()
    {
        var diagnostics = SourceGeneratorTestHelper.RunAnalyzer("""
            using PotternMotching;

            namespace NullablePatternTests;

            public record Thing(string? RuleSetId);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ThingPattern Create() => new(RuleSetId: (string?)null);
            }
            """,
            new PatternDefaultNullLiteralAnalyzer());

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "PM0011");
        Assert.Contains("ValuePattern.Null()", diagnostic.GetMessage());
    }

    [Fact]
    public void NullLiteralAnalyzer_WarnsForBareNullLiteral_AndSuggestsValuePatternNull()
    {
        var diagnostics = SourceGeneratorTestHelper.RunAnalyzer("""
            using PotternMotching;

            namespace NullablePatternTests;

            public record Thing(string? RuleSetId);

            [AutoPatternFor(typeof(Thing))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ThingPattern Create() => new(RuleSetId: null);
            }
            """,
            new PatternDefaultNullLiteralAnalyzer());

        var diagnostic = Assert.Single(diagnostics, d => d.Id == "PM0011");
        Assert.Contains("ValuePattern.Null()", diagnostic.GetMessage());
    }
}
