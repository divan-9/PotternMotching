# PotternMotching

A fluent pattern matching library for .NET that provides powerful patterns for values, collections, and dictionaries with automatic pattern generation from records, structs, and external types.

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

// Match exact items in any order (no extra or missing items)
var exactItems = CollectionPattern.ExactItems(["admin", "editor"]);
exactItems.Evaluate(["editor", "admin"]);  // Success
exactItems.Evaluate(["admin", "editor", "owner"]);  // Failure: wrong length

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

Declare target types with `[AutoPatternFor(typeof(...))]` on a marker class to automatically generate pattern classes with smart defaults:

```csharp
using PotternMotching;

public record Person(string Name, int Age);

public record Address(string City, string Zip);

public record Company(
    string Name,
    Address HeadOffice,
    Address[] Branches,
    HashSet<string> Tags);

[AutoPatternFor(typeof(Person))]
[AutoPatternFor(typeof(Address))]
[AutoPatternFor(typeof(Company))]
internal static class PatternMarkers;
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

The same attribute works for types you do **not** own.

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

Closed generic external targets are also supported:

```csharp
using PotternMotching;
using Shared.Contracts;

namespace MyProject.Tests.Patterns;

[AutoPatternFor(typeof(Result<string>))]
internal static class GenericPatterns;
```

```csharp
using MyProject.Tests.Patterns;

var pattern = new Result_StringPattern(
    Value: "ok"
);
```

### Generation Rules & Limitations

- `[AutoPatternFor]` supports **records**, **classes**, **structs**, **closed generic constructed types**, and **Dunet unions**
- struct targets, including external structs such as `MongoDB.Bson.BsonElement`, are matched through public readable instance properties
- open generic targets such as `typeof(Result<>)` are not supported
- private, protected, and private-protected target types are not supported
- generated pattern accessibility follows the target type: public targets generate public patterns; internal targets generate internal patterns
- for classes, all public instance properties with a public getter may be matched
- records with inheritance are not supported, except normal `object` inheritance
- Dunet union roots generate variant-aware patterns, including closed generic union roots
- non-generic generated type names use `{TypeName}Pattern`
- closed generic generated type names use `{TypeName}_{TypeArg1}_{TypeArg2}...Pattern`
- the generated type is emitted into the **marker type namespace**
- generated pattern constructor parameter names are based on target property names, capitalized and escaped when needed
- nested types are matched as nested patterns only when a pattern is already known; otherwise they fall back to exact value matching
- generated pattern type-name collisions report `PM0009` and must be resolved by changing marker namespaces or target selection
- generated pattern parameter-name collisions report `PM0012`; for example `record Thing(string id, string Id)` cannot generate a valid pattern constructor because both parameters would become `Id`

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

### Null & Nullable Matching

Generated pattern defaults distinguish between **unspecified** and **exact null**:

```csharp
public record MediaOptions(long Width, long Height);
public record FieldOptions(string Name, MediaOptions? Options);

[AutoPatternFor(typeof(MediaOptions))]
[AutoPatternFor(typeof(FieldOptions))]
internal static class PatternMarkers;
```

```csharp
// Options omitted: matches null or non-null values
var anyOptions = new FieldOptionsPattern(Name: "hero");

// Exact null: prefer ValuePattern.Null()
var nullOptions = new FieldOptionsPattern(
    Name: "hero",
    Options: ValuePattern.Null());

// Nullable nested pattern: actual Options must be non-null, then Width is checked
var nonNullOptions = new FieldOptionsPattern(
    Options: new MediaOptionsPattern(Width: 100));
```

Semantics:
- omitted property = match anything
- `ValuePattern.Null()` = exact null matching
- `(T?)null` also works for nullable generated defaults, but `PM0011` suggests `ValuePattern.Null()` for clarity
- nullable nested reference-type properties use `NullablePatternDefault<T, TPattern>`; nullable nested value-type properties use `NullableValuePatternDefault<T, TPattern>`; nested patterns require a non-null actual value
- null collections or dictionaries fail with a normal `MatchResult.Failure` when a collection/dictionary pattern is explicitly specified

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
| Nested pattern-capable types targeted with `[AutoPatternFor]` | `PatternDefault<T, TypePattern>` | Nested pattern matching |
| Nullable nested pattern-capable reference types | `NullablePatternDefault<T, TypePattern>` | Match anything by default; nested patterns require non-null actual values; `ValuePattern.Null()` matches null |
| Nullable nested pattern-capable value types | `NullableValuePatternDefault<T, TypePattern>` | Match anything by default; nested patterns require non-null actual values; `ValuePattern.Null()` matches null |
| Discriminated unions ([Dunet](https://github.com/domn1995/dunet)) | Variant-specific patterns | Variant-aware matching |

## Source Generator Diagnostics

| Diagnostic | Severity | Meaning | Typical fix |
|------------|----------|---------|-------------|
| `PM0001` | Error | Legacy record-only target validation | Use a supported record, class, or struct target through `[AutoPatternFor]` |
| `PM0002` | Error | Record inheritance is not supported | Use a record without a custom base type |
| `PM0003` | Error | Legacy primary-constructor validation | Use a supported positional record, class properties, or struct properties |
| `PM0004` | Warning | Nested type pattern was not found | Add `[AutoPatternFor(typeof(NestedType))]` if nested pattern matching is desired |
| `PM0005` | Error | Dunet union root must be partial | Mark the union root `partial` |
| `PM0006` | Error | Dunet union root has no variants | Add nested record variants |
| `PM0007` | Error | `[AutoPatternFor]` target could not be resolved | Check the `typeof(...)` target |
| `PM0008` | Error | External target is not a class, struct, or record | Use a supported class, struct, record, or Dunet union target |
| `PM0009` | Error | Generated pattern type name collision | Change marker namespace or remove one colliding target |
| `PM0010` | Error | Unsupported target, such as open generic or less-than-internal type | Use a closed generic/public/internal target |
| `PM0011` | Warning | Null literal/cast used for exact null matching | Prefer `ValuePattern.Null()` |
| `PM0012` | Error | Generated pattern constructor parameter name collision | Rename target properties so capitalized pattern parameter names are unique |

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

// Exact items - same length, any order
CollectionPattern.ExactItems(items)

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

Use the `AssertPattern` extension method for concise pattern assertions with automatic expression capture:

```csharp
using PotternMotching;

var person = new Person("Alice", 30);
var pattern = new PersonPattern(Name: "Alice", Age: 30);

person.AssertPattern(pattern);  // Throws AssertionFailedException if no match

// On failure, automatically includes the variable name in the error:
// FAILURE: person.Age: [ValuePattern.Exact] Expected 25, got 30
```

For exact value comparison without explicitly constructing a pattern, use `AssertExact`:

```csharp
person.AssertExact(new Person("Alice", 30));
```

For subset assertions on collections, use `AssertSubset`:

```csharp
var tags = new[] { "important", "backend", "urgent" };

tags.AssertSubset(["important", "urgent"]);
```

For exact unordered assertions on collections, use `AssertExactItems`:

```csharp
var values = new[] { 1, 2, 3 };

values.AssertExactItems([3, 1, 2]);
```

For exact sequence assertions on collections, use `AssertSequence`:

```csharp
var values = new[] { 1, 2, 3 };

values.AssertSequence([1, 2, 3]);
```

For prefix and suffix assertions on collections, use `AssertStartsWith` and `AssertEndsWith`:

```csharp
values.AssertStartsWith([1, 2]);
values.AssertEndsWith([2, 3]);
```

## Advanced: Discriminated Unions

PotternMotching integrates with [Dunet](https://github.com/domn1995/dunet) for discriminated union support:

```csharp
using Dunet;
using PotternMotching;

[Union]
public partial record Job
{
    public partial record Employed(string Company, string Position);
    public partial record Unemployed;
}

public record Person(string Name, Job Job);

[AutoPatternFor(typeof(Job))]
[AutoPatternFor(typeof(Person))]
internal static class PatternMarkers;
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
