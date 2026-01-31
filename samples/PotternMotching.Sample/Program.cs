
using PotternMotching;
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
// Test auto-generated patterns
var autoPattern = new ResultExamplePattern(
    Name: "Alice",
    Age: 30,
    Nicknames: ["Lice1"],
    Addresses: CollectionMatcher.MatchAll([
        new AddressExamplePattern(City: "Wonderland"),
        new AddressExamplePattern(City: "Looking Glass")
    ]));

Console.WriteLine("\nAuto-generated pattern:");
Console.WriteLine(autoPattern.Evaluate(result));

// Additional comprehensive test
Console.WriteLine("\n=== Comprehensive Auto-Generated Pattern Tests ===");

// Test 1: Simple value matching
var testPerson = new TestPerson("Bob", 25);
var testPersonPattern = new TestPersonPattern(Name: "Bob", Age: 25);
Console.WriteLine($"Test Person Match: {testPersonPattern.Evaluate(testPerson)}");

// Test 2: Nested pattern matching
var testCompany = new TestCompany(
    "Acme Corp",
    new TestAddress("New York", "10001"),
    [
        new TestAddress("San Francisco", "94102"),
        new TestAddress("Seattle", "98101")
    ],
    ["tech", "software"]);

var testCompanyPattern = new TestCompanyPattern(
    Name: "Acme Corp",
    HeadOffice: new TestAddressPattern(City: "New York"),
    Branches: [
        new TestAddressPattern(City: "San Francisco"),
        new TestAddressPattern(City: "Seattle")
    ],
    Tags: ["tech", "software"]);

Console.WriteLine($"Test Company Match: {testCompanyPattern.Evaluate(testCompany)}");

// Test 3: Dictionary matching
Console.WriteLine("\n=== Dictionary Matcher Tests ===");

var config = new Dictionary<string, int>
{
    ["timeout"] = 30,
    ["maxRetries"] = 3,
    ["bufferSize"] = 1024
};

// MatchAll - allows extra keys
var matchAllPattern = DictionaryMatcher.MatchAll(new Dictionary<string, IMatcher<int>>
{
    ["timeout"] = ValueMatcher.Exact(30),
    ["maxRetries"] = ValueMatcher.Exact(3)
});
Console.WriteLine($"Dictionary MatchAll (allows extra keys): {matchAllPattern.Evaluate(config)}");

// ExactKeys - requires exact key set
var exactKeysPattern = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
{
    ["timeout"] = ValueMatcher.Exact(30),
    ["maxRetries"] = ValueMatcher.Exact(3),
    ["bufferSize"] = ValueMatcher.Exact(1024)
});
Console.WriteLine($"Dictionary ExactKeys (exact match): {exactKeysPattern.Evaluate(config)}");

// ExactKeys with mismatch
var strictPattern = DictionaryMatcher.ExactKeys(new Dictionary<string, IMatcher<int>>
{
    ["timeout"] = ValueMatcher.Exact(30),
    ["maxRetries"] = ValueMatcher.Exact(3)
});
Console.WriteLine($"Dictionary ExactKeys (missing bufferSize): {strictPattern.Evaluate(config)}");

[AutoPattern]
public record ResultExample(
    string Name,
    int Age,
    HashSet<string> Nicknames,
    AddressExample[] Addresses
);

[AutoPattern]
public record AddressExample(
    string City,
    string Zip
);

// Test types for auto-generated patterns
[AutoPattern]
public record TestPerson(
    string Name,
    int Age);

[AutoPattern]
public record TestAddress(
    string City,
    string Zip);

[AutoPattern]
public record TestCompany(
    string Name,
    TestAddress HeadOffice,
    TestAddress[] Branches,
    HashSet<string> Tags);