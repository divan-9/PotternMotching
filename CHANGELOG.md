# Changelog

All notable changes to this project will be documented in this file.

## [0.4.3] - 2026-07-08

### Added
- `[AutoPatternFor]` now supports **closed generic external targets** such as `typeof(Result<string>)`.
- Generated pattern names for closed generic targets now include concrete type arguments, for example `Result_StringPattern`.
- Added unit coverage for generic target generation.

### Notes
- Open generic targets such as `typeof(Result<>)` remain unsupported.
- Name collisions between generated generic pattern types still report `PM0009` and must be resolved by the consumer.

## [0.4.2] - 2026-06-26

### Fixed
- Generated `Evaluate` no longer produces CS8604 warning for nullable collection properties. Collection property accesses now use null-forgiving operator (`!`) since the runtime pattern types already handle null gracefully.
- Registered `PM0011` diagnostic in analyzer release tracking to fix RS2000 build warning.

## [0.4.1] - 2026-06-26

### Fixed
- Nullable collection implicit conversion no longer crashes on null properties (e.g. `List<T>?`).
- Collection element nullability now preserved in generated patterns (e.g. `List<string?>` → `ValuePattern<string?>`).
- `SetPatternDefault.Evaluate` no longer throws `NullReferenceException` when matcher is `default`.
- Added missing `PM0011` diagnostic descriptor for null literal in nullable pattern defaults.

## [0.4.0]

### Added
- Support for generating patterns for **external records, classes, and Dunet unions** via `[AutoPatternFor(typeof(...))]`.
- New `AutoPatternForAttribute` API for marker-based external type pattern generation.
- Source generator support for emitting external type patterns into the marker type namespace.
- Source generator diagnostics for invalid external targets and generated pattern name collisions.
- Cross-assembly test coverage for external type auto-pattern generation.

### Changed
- Nested pattern resolution now recognizes types targeted through `[AutoPatternFor]`.
- Documentation now includes usage examples for external type pattern generation.
- External class generation now uses public instance properties with a public getter.
- External Dunet union roots now use union-aware pattern generation.

### Notes
- `[AutoPatternFor]` currently supports **records, classes, and Dunet unions**.
- Generated external pattern names are fixed to `{TypeName}Pattern`.
- Unknown nested external types fall back to exact value matching unless a pattern is already known.
