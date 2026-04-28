using PotternMotching;

namespace PotternMotching.Sample.DiagnosticTests;

// This should emit PM0008: Target type must be a class or record
// [AutoPatternFor(typeof(int*))]
// internal static class InvalidMarker;

// This should emit PM0009 if two generated pattern names collide
// [AutoPatternFor(typeof(ValidEmptyRecord))]
// [AutoPatternFor(typeof(PotternMotching.Sample.ValidEmptyRecord))]
// internal static class CollidingMarker;

// These should work fine
public record ValidEmptyRecord();

public record ValidRecord(string Name, int Age);

[AutoPatternFor(typeof(ValidEmptyRecord))]
[AutoPatternFor(typeof(ValidRecord))]
internal static class PatternMarkers;
