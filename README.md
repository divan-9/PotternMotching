# PotternMotching

A fluent pattern matching library for .NET that provides powerful matchers for values, collections, and dictionaries.

[![NuGet](https://img.shields.io/nuget/v/PotternMotching.svg)](https://www.nuget.org/packages/PotternMotching/)
[![Build](https://github.com/divan-9/PotternMotching/actions/workflows/build.yml/badge.svg)](https://github.com/divan-9/PotternMotching/actions/workflows/build.yml)

## Installation

```bash
dotnet add package PotternMotching
```

## Quick Start

### Value Matching

```csharp
using PotternMotching.Matchers;

var matcher = ValueMatcher.Exact(42);
matcher.Evaluate(42);  // Success
matcher.Evaluate(43);  // Failure: Expected 42, got 43
```

### Collection Matching

```csharp
// Match all items in any order
var matcher = CollectionMatcher.MatchAll(["apple", "banana"]);
matcher.Evaluate(["banana", "cherry", "apple"]);  // Success

// Match exact sequence
var sequence = CollectionMatcher.Sequence(["a", "b", "c"]);
sequence.Evaluate(["a", "b", "c"]);  // Success
sequence.Evaluate(["a", "b"]);      // Failure: wrong length
```

### Dictionary Matching

```csharp
// Match specified keys (allows extra keys)
var matchAll = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int>>
{
    ["timeout"] = ValueMatcher.Exact(30)
});
matchAll.Evaluate(new Dictionary<string, int>
{
    ["timeout"] = 30,
    ["retries"] = 3  // Extra keys OK
});  // Success

// Match exact keys (no extra keys allowed)
var exactKeys = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
{
    ["timeout"] = ValueMatcher.Exact(30)
});
exactKeys.Evaluate(new Dictionary<string, int>
{
    ["timeout"] = 30,
    ["retries"] = 3  // Extra key causes failure
});  // Failure: Unexpected key 'retries'
```

### Automatic Pattern Generation

Mark your records with `[AutoPattern]` to automatically generate pattern classes:

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

// Use the generated patterns
var pattern = new CompanyPattern(
    Name: "Acme Corp",
    HeadOffice: new AddressPattern(City: "Seattle"),
    Branches: [
        new AddressPattern(Zip: "98101"),
        new AddressPattern(Zip: "98102")
    ],
    Tags: ["technology", "software"]
);

var company = new Company(
    "Acme Corp",
    new Address("Seattle", "98101"),
    [
        new Address("Portland", "98101"),
        new Address("San Francisco", "98102")
    ],
    ["technology", "software", "cloud"]
);

var result = pattern.Evaluate(company);  // Success
```

## Supported Collection Types

The source generator automatically maps types to appropriate matchers:

| Your Type | Generated Pattern Property Type |
|-----------|--------------------------------|
| `int`, `string`, primitives | `DefaultMatcher<T>` |
| `T[]`, `List<T>`, `IEnumerable<T>` | `DefaultCollectionMatcher<T, DefaultMatcher<T>>` or `DefaultCollectionMatcher<T, PatternType>` |
| `HashSet<T>`, `ISet<T>` | `DefaultCollectionMatcher<T, DefaultMatcher<T>>` or `DefaultCollectionMatcher<T, PatternType>` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `DefaultMatcher<IDictionary<K,V>>` |
| Nested `[AutoPattern]` records | `RecordNamePattern?` |

## Matcher Types

### Value Matchers
- `ValueMatcher.Exact(value)` - Matches exact value using equality

### Collection Matchers
- `CollectionMatcher.MatchAll(items)` - All items must be found (any order). Multiple matchers for same item allowed
- `CollectionMatcher.Sequence(items)` - Exact sequence match (order matters)
- `CollectionMatcher.StartsWith(items)` - Collection must start with items
- `CollectionMatcher.EndsWith(items)` - Collection must end with items

### Dictionary Matchers
- `DictionaryMatcher.MatchAll(pairs)` - All key-value pairs must match (allows extra keys)
- `DictionaryMatcher.ExactKeys(pairs)` - Exact keys match (no extra keys allowed)

## Match Results

All matchers return `MatchResult`:

- `MatchResult.Success` - Pattern matched
- `MatchResult.Failure(reasons)` - Pattern didn't match, with detailed reasons

```csharp
var result = matcher.Evaluate(value);

result.Match(
    success => Console.WriteLine("Matched!"),
    failure => Console.WriteLine($"Failed: {failure}")
);
```

## Requirements

- .NET 10.0 or later
- C# 12 or later (for source generator features)

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.