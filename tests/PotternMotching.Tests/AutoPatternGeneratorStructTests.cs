namespace PotternMotching.Tests;

using Microsoft.CodeAnalysis;
using Xunit;

public class AutoPatternGeneratorStructTests
{
    [Fact]
    public void StructTarget_GeneratesPatternFromPublicReadableProperties()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace StructPatternTests;

            public readonly struct Point
            {
                public Point(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public int X { get; }
                public int Y { get; }
            }

            [AutoPatternFor(typeof(Point))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new PointPattern(X: 1).Evaluate(new Point(1, 2));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void RecordStructTarget_GeneratesPatternFromPublicReadableProperties()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace StructPatternTests;

            public readonly record struct Size(int Width, int Height);

            [AutoPatternFor(typeof(Size))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new SizePattern(Width: 100).Evaluate(new Size(100, 200));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NestedStructProperty_UsesGeneratedStructPattern()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;

            namespace StructPatternTests;

            public readonly record struct Point(int X, int Y);
            public record Shape(Point Center);

            [AutoPatternFor(typeof(Point))]
            [AutoPatternFor(typeof(Shape))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate() =>
                    new ShapePattern(Center: new PointPattern(X: 1)).Evaluate(new Shape(new Point(1, 2)));
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NullableNestedStructProperty_UsesNullableValuePatternDefault()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using PotternMotching;
            using PotternMotching.Patterns;

            namespace StructPatternTests;

            public readonly record struct Point(int X, int Y);
            public record Shape(Point? Center);

            [AutoPatternFor(typeof(Point))]
            [AutoPatternFor(typeof(Shape))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static ShapePattern MatchNestedShape() =>
                    new(Center: new PointPattern(X: 1));

                public static ShapePattern MatchNull() =>
                    new(Center: ValuePattern.Null());

                public static ShapePattern MatchNullableValue() =>
                    new(Center: (Point?)null);
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var generated = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("ShapePattern"));
        Assert.Contains(
            "NullableValuePatternDefault<global::StructPatternTests.Point, global::StructPatternTests.PointPattern> Center = default",
            generated.SourceText.ToString());
    }

    [Fact]
    public void BsonElementTarget_GeneratesPatternFromStructProperties()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using MongoDB.Bson;
            using PotternMotching;

            namespace StructPatternTests;

            [AutoPatternFor(typeof(BsonElement))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static MatchResult Evaluate(BsonElement element) =>
                    new BsonElementPattern(Name: "name").Evaluate(element);
            }
            """);

        Assert.DoesNotContain(result.GeneratorDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.OutputDiagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Contains(result.GeneratedSources, source => source.HintName.Contains("BsonElementPattern"));
    }
}
