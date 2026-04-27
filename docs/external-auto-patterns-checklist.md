# External Record Auto-Pattern Checklist

Scope: **external records only**.

## 1. Runtime API

- [ ] Add `src/PotternMotching/AutoPatternForAttribute.cs`
- [ ] Define `AutoPatternForAttribute(Type targetType)`
- [ ] Expose only `TargetType`
- [ ] Keep `[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]`
- [ ] Build library and ensure no API breaks

---

## 2. Source generator request model

- [ ] Add `src/PotternMotching.SourceGen/Models/GenerationRequest.cs`
- [ ] Include fields/properties for:
  - [ ] `SourceSymbol`
  - [ ] `TargetSymbol`
  - [ ] `GeneratedNamespace`
  - [ ] `GeneratedPatternName`
  - [ ] `IsOwnedType`
- [ ] Use this model for both owned and external generation paths

---

## 3. Generator discovery

File:
- `src/PotternMotching.SourceGen/AutoPatternGenerator.cs`

- [ ] Update syntax/semantic discovery to recognize:
  - [ ] `PotternMotching.AutoPatternAttribute`
  - [ ] `PotternMotching.AutoPatternForAttribute`
- [ ] For `[AutoPattern]`, create a request with:
  - [ ] `TargetSymbol = source type`
  - [ ] `GeneratedNamespace = target namespace`
  - [ ] `GeneratedPatternName = $"{type.Name}Pattern"`
  - [ ] `IsOwnedType = true`
- [ ] For `[AutoPatternFor]`, create one request per attribute instance with:
  - [ ] `SourceSymbol = marker type`
  - [ ] resolved `TargetSymbol`
  - [ ] `GeneratedNamespace = marker namespace`
  - [ ] `GeneratedPatternName = $"{target.Name}Pattern"`
  - [ ] `IsOwnedType = false`
- [ ] Switch generation loop from raw type declarations to requests
- [ ] Use request data when calling analyzer/code generator
- [ ] Use generated pattern name for `context.AddSource(...)`

---

## 4. Diagnostics

File:
- `src/PotternMotching.SourceGen/DiagnosticDescriptors.cs`

- [ ] Add diagnostic for unresolved `[AutoPatternFor]` target
- [ ] Add diagnostic for non-record target
- [ ] Add diagnostic for unsupported/open generic target
- [ ] Add diagnostic for generated name collision
- [ ] Add diagnostics to analyzer release tracking files if needed

Suggested IDs:
- [ ] `PM0007` unresolved external target
- [ ] `PM0008` external target must be a record
- [ ] `PM0009` generated pattern name collision
- [ ] `PM0010` unsupported/open generic external target

---

## 5. Analyzer support for external records

File:
- `src/PotternMotching.SourceGen/TypeAnalyzer.cs`

- [ ] Keep existing owned-type analysis intact
- [ ] Add `AnalyzeExternalRecord(INamedTypeSymbol typeSymbol, Compilation compilation)`
- [ ] Validate target is a record
- [ ] Reject union targets for this MVP
- [ ] Reject open generic/unsupported targets
- [ ] Reuse primary-constructor-based analysis
- [ ] Reuse existing `AnalyzeProperty(...)`
- [ ] Ensure nullability works when syntax references are missing
- [ ] Prefer symbol-based nullability for external metadata types
- [ ] Return diagnostics without breaking owned-type flow

---

## 6. Known-pattern lookup

Files:
- likely `src/PotternMotching.SourceGen/TypeAnalyzer.cs`
- maybe related models/helpers

- [ ] Decide how external analysis identifies nested pattern-capable types
- [ ] Support nested types that already have `[AutoPattern]`
- [ ] Support nested external records also targeted by `[AutoPatternFor]` in current compilation
- [ ] Fall back to `ValuePattern<T>` when nested pattern is unknown
- [ ] Keep this logic conservative for MVP

---

## 7. Code generation overrides

File:
- `src/PotternMotching.SourceGen/PatternCodeGenerator.cs`

- [ ] Allow overriding generated namespace
- [ ] Allow overriding generated pattern name
- [ ] Keep existing generation shape for owned records
- [ ] Reuse the same wrapper mapping rules for external records
- [ ] Ensure generated `Evaluate(...)` uses constructor/property names correctly
- [ ] Ensure generated `From(...)` and `Create(...)` work for external records
- [ ] Ensure implicit conversion operator uses overridden pattern name

Decision:
- [ ] Pass namespace/name overrides as parameters to `Generate(...)`
- [ ] Avoid a broader refactor unless necessary

---

## 8. Collision detection

File:
- `src/PotternMotching.SourceGen/AutoPatternGenerator.cs`

- [ ] Group generation requests by fully-qualified generated type name
- [ ] Detect duplicate outputs before codegen
- [ ] Report diagnostic for collisions
- [ ] Skip conflicting outputs after reporting
- [ ] Keep non-conflicting requests generating normally

---

## 9. External test models project

- [ ] Add `tests/PotternMotching.TestExternalModels/PotternMotching.TestExternalModels.csproj`
- [ ] Add at least one external positional record
- [ ] Add record with collections
- [ ] Add nested external record
- [ ] Add non-record type for negative test coverage
- [ ] Reference this project from `tests/PotternMotching.Tests/PotternMotching.Tests.csproj`

Suggested model set:
- [ ] `ExternalUserDto`
- [ ] `ExternalOrderDto`
- [ ] `ExternalNestedDto`
- [ ] `ExternalNonRecord`

---

## 10. Happy-path tests

Files:
- `tests/PotternMotching.Tests/...`

- [ ] Add test for external positional record generation
- [ ] Add test for matching scalar constructor parameters
- [ ] Add test for arrays/lists/sets/dictionaries on external records
- [ ] Add test for implicit conversion from external record to generated pattern
- [ ] Add test that owned `[AutoPattern]` behavior still works unchanged

---

## 11. Nested-pattern tests

- [ ] Add test where external record contains a known pattern-capable nested type
- [ ] Verify nested type uses generated nested matcher
- [ ] Add test where external record contains unknown external nested record
- [ ] Verify fallback to value matching still compiles and evaluates

---

## 12. Diagnostic tests

- [ ] Add test for `[AutoPatternFor]` targeting non-record type
- [ ] Add test for unresolved/invalid target if practical
- [ ] Add test for generated name collision
- [ ] Verify diagnostics are clear and stable

Note:
- [ ] If diagnostic harness is heavy, defer detailed diagnostic assertions until after happy-path tests land

---

## 13. Build and validation

- [ ] Run `dotnet restore PotternMotching.sln`
- [ ] Run `dotnet test PotternMotching.sln --no-restore`
- [ ] Confirm all existing tests still pass
- [ ] Confirm new external-record tests pass
- [ ] Inspect at least one generated file to verify namespace/name/output shape

---

## 14. Documentation follow-up

- [ ] Update `README.md` after implementation is stable
- [ ] Add one sample usage snippet for external records
- [ ] Document MVP limitations:
  - [ ] records only
  - [ ] fixed naming
  - [ ] marker namespace determines output namespace
  - [ ] no recursive generation for unknown nested external records

---

## Recommended execution order

- [ ] 1. Add runtime attribute
- [ ] 2. Add `GenerationRequest`
- [ ] 3. Update generator discovery
- [ ] 4. Add new diagnostics
- [ ] 5. Implement external record analysis
- [ ] 6. Add namespace/name overrides to codegen
- [ ] 7. Add collision detection
- [ ] 8. Add external models test project
- [ ] 9. Add happy-path tests
- [ ] 10. Add nested-pattern tests
- [ ] 11. Add diagnostic tests
- [ ] 12. Run full test suite
- [ ] 13. Update docs
