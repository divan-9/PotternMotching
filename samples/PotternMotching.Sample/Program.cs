
using PotternMotching;
using PotternMotching.Patterns;
using PotternMotching.Matchers;

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

var examplePattern = new PatternExample(
    Name: "Alice",
    Age: 30,
    Nicknames: ["Lice"],
    Addresses: [
        new(
            City: "Wonderland"
        ),
        new(
            City: "Looking Glass"
        ),
    ]);

var anotherPattern = new PatternExample(
    Name: "Alice",
    Age: 30,
    Nicknames: CollectionMatcher.MatchAll([
        ValueMatcher.Exact("Lice1")
    ]),
    Addresses: CollectionMatcher.EndsWith([
        new AddressExample(
            City: "Looking Glass",
            Zip: "67890"
        ),
        new AddressExample(
            City: "Wonderland",
            Zip: "12345"
        ),
    ]));

Console.WriteLine(examplePattern.Evaluate(result));
Console.WriteLine(anotherPattern.Evaluate(result));

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
    SequencePattern<AddressExample, AddressPatternExample> Addresses = default) : IMatcher<ResultExample>
{
    public MatchResult Evaluate(
        ResultExample value,
        string path = "")
    {
        return MatchResult.Combine([
           this.Name.Value?.Evaluate(value.Name, $"{path}.Name") ?? new MatchResult.Success(),
           this.Age.Value?.Evaluate(value.Age, $"{path}.Age") ?? new MatchResult.Success(),
           this.Nicknames.Values?.Evaluate(value.Nicknames, $"{path}.Nicknames") ?? new MatchResult.Success(),
           this.Addresses.Values?.Evaluate(value.Addresses, $"{path}.Addresses") ?? new MatchResult.Success(),
        ]);
    }
}

public record AddressPatternExample(
    ValuePattern<string> City = default,
    ValuePattern<string> Zip = default) : IMatcher<AddressExample>
{
    public MatchResult Evaluate(
        AddressExample value,
        string path = "")
    {
        return MatchResult.Combine([
           this.City.Value?.Evaluate(value.City, $"{path}.City") ?? new MatchResult.Success(),
           this.Zip.Value?.Evaluate(value.Zip, $"{path}.Zip") ?? new MatchResult.Success(),
        ]);
    }
}