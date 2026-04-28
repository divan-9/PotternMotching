# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
