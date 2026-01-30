
using PatternMatching;
using PatternMatching.PatternBuilders;
using PatternMatching.Patterns;

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
    Nicknames: CollectionPattern.MatchAll(["Lice"]),
    Addresses: CollectionPattern.EndsWith([
        new AddressPatternExample(
            City: "Looking Glass"
        ),
        new AddressPatternExample(
            City: "Wonderland"
        ),
    ]));

var rawPattern = DictionaryPattern.MatchAll<string, IPattern>(new()
{
    ["Name"] = ValuePattern.Exact("Alice"),
    ["Age"] = ValuePattern.Exact(30),
    ["Nicknames"] = CollectionPattern.MatchAll([
        ValuePattern.Exact("Lice"),
    ]),
    ["Addresses"] = CollectionPattern.EndsWith([
        DictionaryPattern.MatchAll<string, IPattern>(new()
        {
            ["City"] = ValuePattern.Exact("Wonderland"),
            ["Zip"] = ValuePattern.Exact("12345")
        }),
        DictionaryPattern.MatchAll<string, IPattern>(new()
        {
            ["City"] = ValuePattern.Exact("Looking Glass"),
            ["Zip"] = ValuePattern.Exact("67890")
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
    ValuePatternBuilder<string> Name = default,
    ValuePatternBuilder<int> Age = default,
    SetPatternBuilder<string> Nicknames = default,
    SequencePatternBuilder<AddressPatternExample> Addresses = default);

public record AddressPatternExample(
    ValuePatternBuilder<string> City = default,
    ValuePatternBuilder<string> Zip = default);