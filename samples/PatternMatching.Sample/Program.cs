
using PatternMatching;
using PatternMatching.Patterns;
using PatternMatching.Matchers;

var result = new ResultExample(
    Name: "Alice",
    Age: 30,
    Nicknames: ["Ally", "Lice"],
    Addresses: [
        new(
            City: "Wonderland",
            Zip: "12345"
        ),
        new(
            City: "Looking Glass",
            Zip: "67890"
        ),
    ]
);

var anotherPattern = new PatternExample(
    Name: "Alice",
    Age: 30,
    Nicknames: CollectionMatcher.MatchAll(["Lice"]),
    Addresses: CollectionMatcher.EndsWith([
        new AddressPatternExample(
            City: "Looking Glass"
        ),
        new AddressPatternExample(
            City: "Wonderland"
        ),
    ]));

var rawPattern = DictionaryMatcher.MatchAll<string, IPattern>(new()
{
    ["Name"] = ValueMatcher.Exact("Alice"),
    ["Age"] = ValueMatcher.Exact(30),
    ["Nicknames"] = CollectionMatcher.MatchAll([
        ValueMatcher.Exact("Lice"),
    ]),
    ["Addresses"] = CollectionMatcher.EndsWith([
        DictionaryMatcher.MatchAll<string, IPattern>(new()
        {
            ["City"] = ValueMatcher.Exact("Wonderland"),
            ["Zip"] = ValueMatcher.Exact("12345")
        }),
        DictionaryMatcher.MatchAll<string, IPattern>(new()
        {
            ["City"] = ValueMatcher.Exact("Looking Glass"),
            ["Zip"] = ValueMatcher.Exact("67890")
        }),
    ])
});

public record ResultExample(
    string Name,
    int Age,
    HashSet<string> Nicknames,
    AddressExample[] Addresses
);

public record AddressExample(
    string City,
    string Zip
);

public record PatternExample(
    ValuePattern<string> Name = default,
    ValuePattern<int> Age = default,
    SetPattern<string> Nicknames = default,
    SequencePattern<AddressPatternExample> Addresses = default);

public record AddressPatternExample(
    ValuePattern<string> City = default,
    ValuePattern<string> Zip = default);