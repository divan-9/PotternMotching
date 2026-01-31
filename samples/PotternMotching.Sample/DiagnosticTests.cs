using PotternMotching;

namespace PotternMotching.Sample.DiagnosticTests;

// This should emit PM0001: Type must be a record
// [AutoPattern]
// public class NotARecord
// {
//     public string Name { get; set; } = "";
// }

// This should emit PM0002: Inheritance not supported
// public record BaseRecord(string Id);
//
// [AutoPattern]
// public record DerivedRecord(string Id, string Name) : BaseRecord(Id);

// These should work fine
[AutoPattern]
public record ValidEmptyRecord();

[AutoPattern]
public record ValidRecord(string Name, int Age);
