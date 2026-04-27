# External Record Auto-Pattern: Minimal Implementation Plan

Scope: **records only**, **fixed naming**, **no user options except target type**.

## Target UX

```csharp
using PotternMotching;
using Shared.Contracts;

namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(ExternalUserDto))]
internal static class TestPatterns;
```

Generates:

```csharp
namespace MyProject.Tests.Patterns;

public sealed record ExternalUserDtoPattern(...)
    : IPattern<ExternalUserDto>, IPatternConstructor<ExternalUserDto>;
```

---

## Rules

- target must be an **external record**
- generation uses the record's **primary constructor parameters**
- generated type name is always:
  - `{TargetType.Name}Pattern`
- generated namespace is always:
  - **marker type namespace**
- no recursion for unknown nested external records
- nested types use pattern matching only when a known pattern already exists

---

## Minimal file changes

### 1. Add runtime attribute

Add file:
- `src/PotternMotching/AutoPatternForAttribute.cs`

Content:

```csharp
namespace PotternMotching;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoPatternForAttribute : Attribute
{
    public AutoPatternForAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    public Type TargetType { get; }
}
```

---

### 2. Add request model for generation

Add file:
- `src/PotternMotching.SourceGen/Models/GenerationRequest.cs`

Minimal model:

```csharp
internal sealed class GenerationRequest
{
    public INamedTypeSymbol SourceSymbol { get; }
    public INamedTypeSymbol TargetSymbol { get; }
    public string GeneratedNamespace { get; }
    public string GeneratedPatternName { get; }
    public bool IsOwnedType { get; }
}
```

Use it for both:
- existing `[AutoPattern]`
- new `[AutoPatternFor]`

---

### 3. Update generator discovery

Change:
- `src/PotternMotching.SourceGen/AutoPatternGenerator.cs`

Minimal approach:

- keep current syntax filtering by attributed type declarations
- detect both attributes:
  - `PotternMotching.AutoPatternAttribute`
  - `PotternMotching.AutoPatternForAttribute`
- for `[AutoPattern]`, create request:
  - `TargetSymbol = source type`
  - `GeneratedNamespace = target namespace`
  - `GeneratedPatternName = $"{type.Name}Pattern"`
  - `IsOwnedType = true`
- for `[AutoPatternFor]`, create one request per attribute instance:
  - `SourceSymbol = marker type`
  - resolve `TargetType` from attribute constructor arg
  - `GeneratedNamespace = marker namespace`
  - `GeneratedPatternName = $"{target.Name}Pattern"`
  - `IsOwnedType = false`

Execution becomes:
- iterate requests, not raw type declarations
- analyze request target
- report diagnostics
- add source using request pattern name

---

### 4. Add minimal diagnostics

Change:
- `src/PotternMotching.SourceGen/DiagnosticDescriptors.cs`

Add only what is needed:

- `PM0007`: target type for `[AutoPatternFor]` must be a record
- `PM0008`: target type for `[AutoPatternFor]` could not be resolved
- `PM0009`: generated pattern name collision
- `PM0010`: open generic or unsupported target type

Keep diagnostics scoped to the new feature.

---

### 5. Reuse analyzer with small extension

Change:
- `src/PotternMotching.SourceGen/TypeAnalyzer.cs`

Goal: avoid broad refactor.

Minimal strategy:

- keep current `Analyze(INamedTypeSymbol typeSymbol, Compilation compilation)` for owned types
- add a second entry point:

```csharp
public static TypeAnalysisResult AnalyzeExternalRecord(
    INamedTypeSymbol typeSymbol,
    Compilation compilation)
```

Behavior:
- require `typeSymbol.IsRecord`
- reject unions for now
- reject unsupported/open generic types
- read primary constructor parameters
- reuse existing `AnalyzeProperty(...)`
- prefer symbol-based nullability when syntax is missing

Important note:
external records may come from metadata, so `DeclaringSyntaxReferences` can be empty.
For external analysis, nullability must not depend on syntax being present.

---

### 6. Allow codegen overrides for namespace/name

Change:
- `src/PotternMotching.SourceGen/PatternCodeGenerator.cs`
- possibly `src/PotternMotching.SourceGen/Models/TypeAnalysisResult.cs`

Minimal approach:

Either:

#### Option A
Pass overrides directly:

```csharp
PatternCodeGenerator.Generate(
    analysis,
    generatedNamespace,
    generatedPatternName)
```

#### Option B
Store them in `TypeAnalysisResult`

```csharp
public string GeneratedNamespace { get; }
public string GeneratedPatternName { get; }
```

For minimal change, **Option A is simpler**.

Generator should stop deriving these only from `analysis.TypeSymbol`.

---

### 7. Add collision detection

Change:
- `src/PotternMotching.SourceGen/AutoPatternGenerator.cs`

Before generating source:
- group requests by:
  - `GeneratedNamespace`
  - `GeneratedPatternName`
- if more than one request produces the same fully qualified generated type, report diagnostic and skip those conflicting outputs

This replaces the removed `PatternName` customization.

---

## Test plan

### Add external-model test project

Add project:
- `tests/PotternMotching.TestExternalModels/PotternMotching.TestExternalModels.csproj`

Put record types there, for example:
- `ExternalUserDto`
- `ExternalOrderDto`
- `ExternalNestedDto`

Then reference it from:
- `tests/PotternMotching.Tests/PotternMotching.Tests.csproj`

This is important because the feature must be tested against **another assembly**.

---

### Add tests

Add file(s) under:
- `tests/PotternMotching.Tests/`

Minimal cases:

1. **External positional record works**
   - marker type in test assembly
   - generated pattern compiles and evaluates successfully

2. **Collections on external record work**
   - array/list/set/dictionary constructor parameters map correctly

3. **Nested known pattern works**
   - external record contains owned pattern-capable record

4. **Nested unknown external record falls back to value matching**
   - ensure generation still compiles and evaluates

5. **Non-record target reports diagnostic**
   - compile-time diagnostic test

6. **Collision reports diagnostic**
   - two markers in same namespace target different `User` records with same simple name

If diagnostic testing is too heavy for first pass, start with runtime compilation/integration coverage and add diagnostic-specific tests after.

---

## Recommended implementation order

1. add `AutoPatternForAttribute`
2. add `GenerationRequest`
3. update `AutoPatternGenerator` to build requests
4. add `AnalyzeExternalRecord(...)`
5. update `PatternCodeGenerator` to accept namespace/name overrides
6. add collision detection
7. add one happy-path test with a real external record
8. add collection/nested tests
9. add diagnostic tests

---

## Keep out of scope

Do **not** implement in this pass:

- external classes
- include/exclude member selection
- user-selected pattern names
- recursive external type generation
- external unions
- custom property/member selection rules

---

## Success criteria

The first draft is done when all of the following are true:

- `[AutoPatternFor(typeof(SomeExternalRecord))]` works
- generated type is top-level in marker namespace
- external record constructor parameters are mapped like owned records
- current `[AutoPattern]` behavior is unchanged
- collisions produce a diagnostic instead of broken output
- tests prove it works across assembly boundaries
