namespace PotternMotching.Tests;

using System.Linq;
using Xunit;

public class AutoPatternGeneratorUnionRootImplicitConversionTests
{
    [Fact]
    public void UnionPattern_GeneratesImplicitConversionFromUnionRoot()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("""
            using System.Collections.Generic;
            using Dunet;
            using PotternMotching;

            namespace UnionRootCollectionTests;

            [Union]
            public abstract partial record FieldOptions
            {
                public partial record Text(string Name);
                public partial record Url(string Name);
            }

            public record SharingOptions(
                int MinWinners,
                int MaxWinners);

            public record FormatOptions(
                string PlacementId,
                string FormatId,
                bool StopImpressions,
                SharingOptions SharingOptions,
                IReadOnlyCollection<FieldOptions> FieldOptions);

            [AutoPatternFor(typeof(FieldOptions))]
            [AutoPatternFor(typeof(SharingOptions))]
            [AutoPatternFor(typeof(FormatOptions))]
            internal static class PatternMarkers;

            public static class Usage
            {
                public static FormatOptionsPattern Create(FormatOptions formatOptions) =>
                    new(
                        PlacementId: formatOptions.PlacementId,
                        FormatId: formatOptions.FormatId,
                        StopImpressions: formatOptions.StopImpressions,
                        SharingOptions: formatOptions.SharingOptions,
                        FieldOptions: [.. formatOptions.FieldOptions]);
            }
            """);

        var generated = Assert.Single(result.GeneratedSources, source => source.HintName.Contains("FieldOptionsPattern"));

        Assert.Contains("public static implicit operator FieldOptionsPattern(", generated.SourceText.ToString());
        Assert.Contains("UnionRootCollectionTests.FieldOptions value", generated.SourceText.ToString());
        Assert.Contains("return (FieldOptionsPattern)From(value);", generated.SourceText.ToString());
    }
}
