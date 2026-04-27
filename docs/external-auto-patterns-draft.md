# External Record Auto-Pattern Generation Draft

## Status

Draft v1, narrowed to **records only**.

This document proposes a first implementation for generating patterns for record types we do not own.

---

## Problem

Today `PotternMotching` can generate patterns only for types marked with `[AutoPattern]`.
That works well for records defined in the current compilation, but it does not help when tests need a generated pattern for a record from:

- a shared library we cannot modify
- a third-party package
- another project where adding `PotternMotching` attributes is not desirable

Current workarounds are:

1. write `IPattern<T>` by hand
2. create a local adapter record and map the external type into it

Both work, but neither is as convenient as a generated pattern type.

---

## Goal

Allow users to generate a pattern for an **external record** without modifying that record.

Desired usage:

```csharp
using PotternMotching;

namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(ExternalUserDto))]
internal static partial class TestPatterns;
```

Generated result:

```csharp
namespace MyProject.Tests.Patterns;

public sealed record ExternalUserDtoPattern(
    PatternDefault<string, ValuePattern<string>> Id = default,
    PatternDefault<string, ValuePattern<string>> Name = default,
    SequencePatternDefault<string, PatternDefault<string, ValuePattern<string>>> Roles = default
) : IPattern<ExternalUserDto>, IPatternConstructor<ExternalUserDto>;
```

Usage:

```csharp
user.Assert(new ExternalUserDtoPattern(
    Id: "42",
    Roles: ["admin", "editor"]
));
```

---

## Why records only for v1

Restricting the feature to records keeps it aligned with the current architecture.

The current generator already understands:

- records
- primary constructor parameters
- wrapper inference from constructor parameter types
- nested generated patterns

That means external record support can reuse most of the existing analyzer/codegen logic, while class/property-based support would require a broader redesign.

---

## Non-goals for the first version

Out of scope for MVP:

- external classes
- non-record types
- property-based generation for arbitrary objects
- recursive generation for all nested external types automatically
- open generic targets
- fields
- methods
- indexers
- custom matcher selection per member
- external union support unless a concrete need appears

---

## Proposed API

### New attribute

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

### Attribute target

The attribute is applied to a local marker type.

Recommended shape:

```csharp
[AutoPatternFor(typeof(ExternalUserDto))]
internal static partial class TestPatterns;
```

Why a marker type instead of assembly-level attribute:

- gives us a namespace for generated output
- gives a stable place for future options
- supports multiple declarations in a project
- avoids generating into third-party namespaces

---

## Generation rules

### Supported target type

For MVP, the target must be:

- a record
- a named type
- not an open generic type
- not a union handled through the separate Dunet path

### Record shape

For MVP, generation should use the record's **primary constructor parameters**, just like current `[AutoPattern]` generation.

That means:

- constructor parameters define generated pattern members
- evaluation reads the matching generated record properties
- records without a usable primary constructor are out of scope for this first version

### Namespace

Generated pattern should use the namespace of the marker type.

Example:

```csharp
namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(ExternalUserDto))]
internal static partial class TestPatterns;
```

Generates:

```csharp
namespace MyProject.Tests.Patterns;
public sealed record ExternalUserDtoPattern(...)
```

### Pattern name

Fixed rule:

- `{TargetType.Name}Pattern`

No user-configurable naming is proposed for MVP.

### Wrapper mapping

Use the same mapping rules as current `[AutoPattern]` generation:

- scalar/value types -> `PatternDefault<T, ValuePattern<T>>`
- `T[]`, `List<T>`, `IEnumerable<T>` -> `SequencePatternDefault<T, ...>`
- `HashSet<T>`, `ISet<T>` -> `SetPatternDefault<T, ...>`
- `Dictionary<TKey, TValue>`, `IDictionary<TKey, TValue>` -> `DictionaryPatternDefault<TKey, TValue, ...>`
- nested type with known generated pattern -> use that pattern as matcher
- otherwise -> fall back to value matching

### Nested type behavior

For MVP, nested external record types should not trigger automatic recursive generation.

Behavior:

- if nested type already has a known pattern, use it
- otherwise use exact matching via `ValuePattern<T>`

This keeps the feature predictable and close to today's behavior.

---

## Examples

### Example 1: external positional record

```csharp
namespace Shared.Contracts;

public record ExternalUserDto(
    string Id,
    string Name,
    string[] Roles);
```

```csharp
namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(ExternalUserDto))]
internal static partial class TestPatterns;
```

Expected generated constructor:

```csharp
new ExternalUserDtoPattern(
    Id: "42",
    Name: "Alice",
    Roles: ["admin"]
)
```

### Example 2: external record with nested owned pattern

```csharp
namespace Shared.Contracts;

public record ExternalOrderDto(
    string Id,
    Money Total);
```

If `Money` is already pattern-capable in the current compilation, generate:

```csharp
public sealed record ExternalOrderDtoPattern(
    PatternDefault<string, ValuePattern<string>> Id = default,
    PatternDefault<Money, MoneyPattern> Total = default
)
```

---

## Diagnostics

New diagnostics suggested for this feature:

### Error: target type could not be resolved

When `typeof(...)` cannot be resolved to a valid type symbol.

### Error: target type must be a record

If `[AutoPatternFor]` points to a class, struct, interface, enum, or other unsupported type.

### Error: target record must have a supported primary constructor

If the target record cannot be analyzed through the existing constructor-based flow.

### Error: unsupported target type

For unsupported cases such as:

- open generics
- pointer types
- function pointers

### Error: generated pattern name collision

If two declarations would generate the same fully qualified pattern type.

---

## Implementation draft

## 1. Runtime attribute

Add:

- `src/PotternMotching/AutoPatternForAttribute.cs`

Minimal API for v1:

- `TargetType`

---

## 2. Generator discovery changes

Current discovery in `src/PotternMotching.SourceGen/AutoPatternGenerator.cs` only looks for type declarations annotated with `[AutoPattern]`.

Extend discovery to also detect marker types annotated with `[AutoPatternFor]`.

Generator input should distinguish between:

- owned source type generation
- external record generation

Suggested internal request model:

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

For `[AutoPattern]`:

- `SourceSymbol == TargetSymbol`
- `GeneratedNamespace == target namespace`
- `GeneratedPatternName == $"{TargetSymbol.Name}Pattern"`
- `IsOwnedType == true`

For `[AutoPatternFor]`:

- `SourceSymbol == marker type`
- `TargetSymbol == external record`
- `GeneratedNamespace == marker namespace`
- `GeneratedPatternName == $"{TargetSymbol.Name}Pattern"`
- `IsOwnedType == false`

---

## 3. Analyzer changes

This is where narrowing to records helps.

Current `TypeAnalyzer` is already record/constructor oriented. For v1, we should try to **reuse it**, not replace it.

### Owned type analysis

Keep current logic for:

- records with primary constructor
- Dunet unions

### External record analysis

Add a constrained path:

- validate `TargetSymbol.IsRecord`
- reject union records for now
- analyze primary constructor parameters from `TargetSymbol`
- reuse existing wrapper-kind inference per constructor parameter

This should avoid a large refactor toward arbitrary property/member analysis.

### Nullable handling

External records may come from metadata, so syntax-based nullability extraction may not be available.

For external records, prefer symbol-based nullability:

- `parameter.NullableAnnotation`
- `parameter.Type.NullableAnnotation`

If syntax exists, current logic can still use it.

---

## 4. Code generation changes

`src/PotternMotching.SourceGen/PatternCodeGenerator.cs` already contains most of the needed wrapper logic.

Main change for v1:

- allow generation from a request that can override namespace and pattern name
- keep member generation based on analyzed constructor parameters

That means the generated body can stay very close to the current record generation output.

Example evaluation output:

```csharp
return MatchResult.Combine([
    this.Id.Evaluate(value.Id, $"{path}.Id"),
    this.Name.Evaluate(value.Name, $"{path}.Name"),
    this.Roles.Evaluate(value.Roles, $"{path}.Roles")
]);
```

Implicit conversion should also follow the same shape as current generated record patterns:

```csharp
public static implicit operator ExternalUserDtoPattern(ExternalUserDto value)
{
    return new ExternalUserDtoPattern(
        Id: value.Id,
        Name: value.Name,
        Roles: new SequencePatternDefault<string, PatternDefault<string, ValuePattern<string>>>(
            SequencePatternDefault<string, PatternDefault<string, ValuePattern<string>>>.From(value.Roles))
    );
}
```

---

## 5. Pattern lookup

The generator needs a notion of "known generated pattern type" when deciding whether a nested constructor parameter can use a nested pattern instead of plain value matching.

For MVP, the lookup can be conservative.

A nested type is considered pattern-capable if:

- it has `[AutoPattern]`, or
- it is targeted by `[AutoPatternFor]` in the current compilation

If not, fall back to `ValuePattern<T>`.

---

## 6. Tests

Suggested test coverage:

### Unit/integration tests

1. generates pattern for external positional record with scalar parameters
2. generates pattern for external record with array/list/set/dictionary parameters
3. reports diagnostic when target is not a record
4. reports diagnostic for unsupported/open generic target
5. reports diagnostic for name collision
6. generates in marker namespace, not target namespace
7. nested known pattern uses nested matcher
8. nested unknown external record falls back to exact/value matcher
9. nullable reference parameters preserve `?`

### Sample update

Add one real external-record example to `samples/PotternMotching.Sample/Program.cs` after MVP works.

---

## Compatibility and migration

This feature is additive.

No breaking changes are expected if:

- existing `[AutoPattern]` flow stays intact
- new diagnostics are scoped to the new attribute only

---

## Risks

### 1. External record metadata may behave slightly differently

Nullability and constructor details may be available through metadata rather than syntax.

Mitigation:

- prefer symbol-based analysis for external targets
- keep syntax-based logic as an optional enhancement when available

### 2. Name collisions

Two different external records may share the same simple name.

Mitigation:

- add an explicit diagnostic on collisions
- keep naming fixed and deterministic for MVP
- if separation is needed, place markers in different namespaces

### 3. Nested fallback may be weaker than users expect

If nested external records use `ValuePattern<T>`, record equality semantics apply instead of partial nested matching.

Mitigation:

- document this clearly
- leave recursive external generation for a later phase

---

## Open questions

1. Should we support only positional records in MVP?
   - recommendation: yes, effectively yes; stay on the primary-constructor path

2. Should marker type be required to be `partial`?
   - recommendation: no strong need if we do not generate into the marker itself

3. Should we support multiple `[AutoPatternFor]` attributes on one marker type?
   - recommendation: yes

4. Should external unions be supported immediately?
   - recommendation: no

---

## Recommended delivery plan

### Phase 1

- add `AutoPatternForAttribute`
- add discovery for marker-based external record requests
- validate target record shape
- reuse existing constructor-based analysis for external records
- add diagnostics for invalid targets and collisions

### Phase 2

- improve nested pattern lookup for external records
- documentation and sample updates

### Phase 3

- consider class/property-based generation separately, as a different feature
- consider deeper recursive external generation only if needed

---

## Summary

Recommended first implementation:

- add `[AutoPatternFor(typeof(SomeExternalRecord))]`
- place it on a local marker type
- support **records only**
- generate `{TargetType.Name}Pattern` in the marker namespace
- reuse existing primary-constructor-based analysis
- keep recursive generation out of MVP

This gives a practical solution for tests while staying close to the current generator design.