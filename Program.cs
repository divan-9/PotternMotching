
using PatternMatching;
using PatternMatching.RuleBuilders;

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
    Nicknames: CollectionRule.MatchAll(["Lice"]),
    Addresses: CollectionRule.EndsWith([
        new AddressPatternExample(
            City: "Looking Glass"
        ),
        new AddressPatternExample(
            City: "Wonderland"
        ),
    ]));

var rawPattern = DictionaryRule.MatchAll<string, IRule>(new()
{
    ["Name"] = ValueRule.Exact("Alice"),
    ["Age"] = ValueRule.Exact(30),
    ["Nicknames"] = CollectionRule.MatchAll([
        ValueRule.Exact("Lice"),
    ]),
    ["Addresses"] = CollectionRule.EndsWith([
        DictionaryRule.MatchAll<string, IRule>(new()
        {
            ["City"] = ValueRule.Exact("Wonderland"),
            ["Zip"] = ValueRule.Exact("12345")
        }),
        DictionaryRule.MatchAll<string, IRule>(new()
        {
            ["City"] = ValueRule.Exact("Looking Glass"),
            ["Zip"] = ValueRule.Exact("67890")
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
    ValueRuleBuilder<string> Name = default,
    ValueRuleBuilder<int> Age = default,
    SetRuleBuilder<string> Nicknames = default,
    SequenceBuilder<AddressPatternExample> Addresses = default);

public record AddressPatternExample(
    ValueRuleBuilder<string> City = default,
    ValueRuleBuilder<string> Zip = default);