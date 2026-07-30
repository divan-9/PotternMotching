namespace PotternMotching.Tests;

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using PotternMotching.SourceGen;

internal static class SourceGeneratorTestHelper
{
    public static GeneratorTestResult RunGenerator(string source, string assemblyName = "PotternMotching.SourceGen.Tests")
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [syntaxTree],
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new AutoPatternGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);

        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToImmutableArray();

        return new GeneratorTestResult(
            runResult.Diagnostics,
            outputCompilation.GetDiagnostics().AddRange(outputDiagnostics),
            generatedSources,
            outputCompilation);
    }

    public static ImmutableArray<Diagnostic> RunAnalyzer(string source, DiagnosticAnalyzer analyzer, string assemblyName = "PotternMotching.SourceGen.Tests")
    {
        var generatorResult = RunGenerator(source, assemblyName);
        var compilationWithAnalyzers = generatorResult.OutputCompilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToImmutableArray();
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        _ = typeof(object);
        _ = typeof(Enumerable);
        _ = typeof(AutoPatternForAttribute);
        _ = typeof(AutoPatternGenerator);

        LoadReferencedAssemblies(typeof(PotternMotching.TestExternalModels.ExternalJob).Assembly);

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(static assembly => assembly.Location)
            .Distinct()
            .Select(static location => MetadataReference.CreateFromFile(location))
            .ToArray();
    }

    private static void LoadReferencedAssemblies(Assembly assembly)
    {
        foreach (var assemblyName in assembly.GetReferencedAssemblies())
        {
            try
            {
                _ = Assembly.Load(assemblyName);
            }
            catch
            {
                // Best-effort only.
            }
        }
    }
}

internal sealed record GeneratorTestResult(
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> OutputDiagnostics,
    ImmutableArray<GeneratedSourceResult> GeneratedSources,
    Compilation OutputCompilation);
