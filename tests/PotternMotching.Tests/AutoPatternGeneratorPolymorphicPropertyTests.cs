namespace PotternMotching.Tests;

using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorPolymorphicPropertyTests
{
    [Fact]
    public void InterfaceTypedProperty_AcceptsConcreteGeneratedPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PolymorphicPatternTests;

            public interface IFragmentTemplate;

            public record StringFragmentTemplate(string Value) : IFragmentTemplate;
            public record BannerFragmentTemplate(string Url) : IFragmentTemplate;

            public record ImpressionRule(IFragmentTemplate FragmentTemplate);

            [AutoPatternFor(typeof(StringFragmentTemplate))]
            [AutoPatternFor(typeof(BannerFragmentTemplate))]
            [AutoPatternFor(typeof(ImpressionRule))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ImpressionRulePattern Create() => new(
                    FragmentTemplate: new StringFragmentTemplatePattern(Value: "xxxx"));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void InterfaceTypedProperty_WholeObjectImplicitConversion_Compiles()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PolymorphicPatternTests;

            public interface IFragmentTemplate;

            public record StringFragmentTemplate(string Value) : IFragmentTemplate;
            public record ImpressionRule(IFragmentTemplate FragmentTemplate);

            [AutoPatternFor(typeof(StringFragmentTemplate))]
            [AutoPatternFor(typeof(ImpressionRule))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ImpressionRulePattern Create(ImpressionRule value) => value;
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void AbstractBaseTypedProperty_AcceptsConcreteGeneratedPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace PolymorphicPatternTests;

            public abstract class FragmentTemplate;

            public class StringFragmentTemplate : FragmentTemplate
            {
                public StringFragmentTemplate(string value) => Value = value;
                public string Value { get; }
            }

            public class BannerFragmentTemplate : FragmentTemplate
            {
                public BannerFragmentTemplate(string url) => Url = url;
                public string Url { get; }
            }

            public record ImpressionRule(FragmentTemplate FragmentTemplate);

            [AutoPatternFor(typeof(StringFragmentTemplate))]
            [AutoPatternFor(typeof(BannerFragmentTemplate))]
            [AutoPatternFor(typeof(ImpressionRule))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ImpressionRulePattern Create() => new(
                    FragmentTemplate: new StringFragmentTemplatePattern(Value: "xxxx"));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }
}
