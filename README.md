# PotternMotching

A fluent pattern matching library for .NET that provides powerful patterns for values, collections, and dictionaries with automatic pattern generation from records and external types.

[![NuGet](https://img.shields.io/nuget/v/PotternMotching.svg)](https://www.nuget.org/packages/PotternMotching/)
[![Build](https://github.com/divan-9/PotternMotching/actions/workflows/build.yml/badge.svg)](https://github.com/divan-9/PotternMotching/actions/workflows/build.yml)

## Installation

```bash
dotnet add package PotternMotching
```

## Quick Start

### Value Matching

```csharp
using PotternMotching.Patterns;

var pattern = ValuePattern.Exact(42);
pattern.Evaluate(42);  // Success
pattern.Evaluate(43);  // Failure: Expected 42, got 43
```

### Collection Matching

```csharp
// Match all items in any order (subset matching)
var pattern = CollectionPattern.Subset(["apple", "banana"]);
pattern.Evaluate(["banana", "cherry", "apple"]);  // Success - all patterns found

// Match exact sequence
var sequence = CollectionPattern.Sequence(["a", "b", "c"]);
sequence.Evaluate(["a", "b", "c"]);  // Success
sequence.Evaluate(["a", "b"]);       // Failure: wrong length

// Match prefix
var prefix = CollectionPattern.StartsWith(["hello", "world"]);
prefix.Evaluate(["hello", "world", "!"]);  // Success

// Match suffix
var suffix = CollectionPattern.EndsWith(["!", "!"]);
suffix.Evaluate(["wow", "!", "!"]);  // Success

// Match any element
var anyMatch = CollectionPattern.AnyElement("target");
anyMatch.Evaluate(["foo", "target", "bar"]);  // Success
```

### Dictionary Matching

```csharp
using PotternMotching.Patterns;

// Match specified keys (allows extra keys)
var pattern = DictionaryPattern.Items(new Dictionary<string, IPattern<int>>
{
    ["timeout"] = ValuePattern.Exact(30)
});
pattern.Evaluate(new Dictionary<string, int>
{
    ["timeout"] = 30,
    ["retries"] = 3  // Extra keys OK
});  // Success

// Match exact keys (no extra keys allowed)
var exactPattern = DictionaryPattern.ExactItems(new Dictionary<string, IPattern<int>>
{
    ["timeout"] = ValuePattern.Exact(30)
});
exactPattern.Evaluate(new Dictionary<string, int>
{
    ["timeout"] = 30,
    ["retries"] = 3  // Extra key causes failure
});  // Failure: Unexpected keys: 'retries'
```

## Automatic Pattern Generation

Mark your records with `[AutoPattern]` to automatically generate pattern classes with smart defaults:

```csharp
using PotternMotching;

[AutoPattern]
public record Person(string Name, int Age);

[AutoPattern]
public record Address(string City, string Zip);

[AutoPattern]
public record Company(
    string Name,
    Address HeadOffice,
    Address[] Branches,
    HashSet<string> Tags);
```

The source generator creates pattern classes you can use immediately:

```csharp
// Create patterns - all properties are optional (default = match anything)
var pattern = new CompanyPattern(
    Name: "Acme Corp",                              // Match exact name
    HeadOffice: new AddressPattern(City: "Seattle"), // Partial match - any zip
    Branches: [                                      // Sequence match (exact order & length)
        new AddressPattern(Zip: "98101"),
        new AddressPattern(Zip: "98102")
    ],
    Tags: ["technology", "software"]                 // Subset match (unordered)
);

var company = new Company(
    "Acme Corp",
    new Address("Seattle", "98101"),
    [
        new Address("Portland", "98101"),
        new Address("San Francisco", "98102")
    ],
    ["technology", "software", "cloud"]  // Extra tag is OK for HashSet
);

var result = pattern.Evaluate(company);  // Success
```

### External Types

You can also generate patterns for types you do **not** own.

```csharp
// In another assembly
namespace Shared.Contracts;

public class ExternalUserDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string[] Roles { get; init; } = [];
}
```

```csharp
// In your test project or consumer project
using PotternMotching;
using Shared.Contracts;

namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(ExternalUserDto))]
internal static class ExternalPatterns;
```

This generates a top-level pattern type in the marker namespace:

```csharp
using MyProject.Tests.Patterns;

var pattern = new ExternalUserDtoPattern(
    Id: "42",
    Roles: ["admin"]
);
```

Notes:
- `[AutoPatternFor]` supports external **records**, **classes**, and external **Dunet unions**
- for classes, all public instance properties with a public getter may be matched
- external Dunet union roots generate variant-aware patterns just like owned unions
- the generated type name is always `{TypeName}Pattern`
- the generated type is emitted into the **marker type namespace**
- nested external types are matched as nested patterns only when a pattern is already known; otherwise they fall back to exact value matching

### Flexible Matching with Defaults

Properties you don't specify match anything:

```csharp
// Only check the name and city
var flexiblePattern = new CompanyPattern(
    Name: "Acme Corp",
    HeadOffice: new AddressPattern(City: "Seattle")
    // Branches and Tags not specified - will match any value
);
```

### Collection Expressions & Implicit Conversions

Generated patterns support C# collection expressions and implicit conversions:

```csharp
// Mix patterns and values in collection literals
SequencePatternDefault<Address, AddressPattern> branches = [
    new AddressPattern(City: "Portland"),      // Pattern
    new Address("Seattle", "98101"),           // Value - implicitly converted!
];

// Convert entire objects to patterns
CompanyPattern pattern = company;  // Implicit conversion
```

## Supported Types for Auto-Pattern Generation

The source generator automatically maps types to appropriate pattern wrappers:

| Your Type | Generated Pattern Property Type | Matching Behavior |
|-----------|--------------------------------|-------------------|
| `int`, `string`, primitives | `PatternDefault<T, ValuePattern<T>.Exact>` | Exact equality |
| `T[]`, `List<T>`, `IEnumerable<T>` | `SequencePatternDefault<T, ...>` | Exact sequence (order + length) |
| `HashSet<T>`, `ISet<T>` | `SetPatternDefault<T, ...>` | Subset (unordered, allows extras) |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `DictionaryPatternDefault<K,V, ...>` | Key-value pairs (allows extra keys) |
| Nested pattern-capable records (`[AutoPattern]` or `[AutoPatternFor]`) | `RecordNamePattern?` | Nested pattern matching |
| Discriminated unions ([Dunet](https://github.com/domn1995/dunet)) | Variant-specific patterns | Variant-aware matching |

## Pattern Types Reference

### Value Patterns
```csharp
using PotternMotching.Patterns;

// Exact equality matching
ValuePattern.Exact(value)
```

### Collection Patterns
```csharp
using PotternMotching.Patterns;

// All patterns must be found in any order (allows extras)
CollectionPattern.Subset(items)

// Exact sequence - same order and length
CollectionPattern.Sequence(items)

// Collection must start with these items
CollectionPattern.StartsWith(items)

// Collection must end with these items
CollectionPattern.EndsWith(items)

// At least one element must match
CollectionPattern.AnyElement(pattern)
```

All collection pattern factory methods accept either arrays of values or arrays of patterns.

### Dictionary Patterns
```csharp
using PotternMotching.Patterns;

// All specified key-value pairs must match (allows extra keys)
DictionaryPattern.Items(pairs)

// Exact keys - no more, no less (no extra keys allowed)
DictionaryPattern.ExactItems(pairs)
```

## Match Results & Error Messages

All patterns return a `MatchResult` - a discriminated union with detailed error information:

```csharp
var result = pattern.Evaluate(value);

result.Match(
    success => Console.WriteLine("Matched!"),
    failure => Console.WriteLine($"Failed:\n{failure}")
);
```

**MatchResult** has two cases:
- `MatchResult.Success` - Pattern matched successfully
- `MatchResult.Failure(string[] Reasons)` - Pattern didn't match, with detailed path-based reasons

### Detailed Error Messages

PotternMotching provides precise error messages with full paths:

```csharp
var pattern = new CompanyPattern(
    Branches: [
        new AddressPattern(Zip: "98101"),
        new AddressPattern(Zip: "98102")
    ]
);

var company = new Company(
    "Acme",
    null!,
    [
        new Address("Portland", "99999"),  // Wrong zip!
        new Address("Seattle", "98102")
    ],
    []
);

var result = pattern.Evaluate(company);
// Failure:
//   - .Branches[0].Zip: [ValuePattern.Exact] Expected 98101, got 99999
```

## Test Assertions

Use the `Assert` extension method for concise test assertions with automatic expression capture:

```csharp
using PotternMotching;

var person = new Person("Alice", 30);
var pattern = new PersonPattern(Name: "Alice", Age: 30);

person.Assert(pattern);  // Throws AssertionFailedException if no match

// On failure, automatically includes the variable name in the error:
// FAILURE: person.Age: [ValuePattern.Exact] Expected 25, got 30
```

## Advanced: Discriminated Unions

PotternMotching integrates with [Dunet](https://github.com/domn1995/dunet) for discriminated union support:

```csharp
using Dunet;
using PotternMotching;

[Union]
[AutoPattern]
public partial record Job
{
    public partial record Employed(string Company, string Position);
    public partial record Unemployed;
}

[AutoPattern]
public record Person(string Name, Job Job);
```

Generated patterns are variant-aware:

```csharp
var pattern = new PersonPattern(
    Name: "Alice",
    Job: new JobPattern.Employed(
        Company: "Tech Corp",
        Position: "Developer"
    )
);

var person = new Person("Alice", new Job.Employed("Tech Corp", "Developer"));
var result = pattern.Evaluate(person);  // Success

var unemployed = new Person("Bob", new Job.Unemployed());
var result2 = pattern.Evaluate(unemployed);
// Failure: .Job: Expected variant Employed, got Unemployed
```

## Advanced: Custom Patterns

Override default matching behavior by providing explicit patterns:

```csharp
var pattern = new CompanyPattern(
    Name: "Acme Corp",
    // Use CollectionPattern.StartsWith instead of default Sequence
    Branches: CollectionPattern.StartsWith([
        new AddressPattern(City: "Seattle")
    ]),
    // Use custom pattern for Tags
    Tags: CollectionPattern.AnyElement("important")
);
```

## Requirements

- .NET 10.0 or later
- C# 12 or later (for collection expressions and source generator features)

## Examples

See the [samples directory](samples/PotternMotching.Sample) for complete working examples.

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
