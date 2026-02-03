
using Dunet;
using PotternMotching;
using PotternMotching.Patterns;

var result = new Person(
    Name: "Alice",
    Age: 30,
    Nicknames: ["Ally", "Lice"],
    Address: new Address(
        City: "Wonderland",
        Zip: "12345"
    ),
    Job: new Job.Employed(
        Company: "Tech Corp",
        Position: "Developer"
    ),
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

// Demonstrate implicit conversion: can use Address values directly in collection literals
SequencePatternDefault<Address, AddressPattern> addressMatcher = [
    new AddressPattern(City: "Wonderland"),          // Pattern type
    new Address(City: "Wonderland", Zip: "12345"),   // Value type - implicitly converted!
];

PatternDefault<Job, JobPattern> testMatcher = new JobPattern.Employed(
    Company: "Tech Corp",
    Position: "Developer"
);

// Can create patterns manually
var pattern = new PersonPattern(
    Name: "Alice",
    Age: 30,
    Address: new AddressPattern(City: "Wonderland"),
    Job: new JobPattern.Employed(
        Company: "Tech Corp"),
    Nicknames: CollectionPattern.AnyElement("Ally"),
    Addresses: CollectionPattern.AnyOrder([
        new Address(City: "Looking Glass", Zip: "67890"),  // Value type - implicitly converted!
        new Address(City: "Wonderland", Zip: "12345")      // Value type - implicitly converted!,
    ]));

// Or convert entire Person objects to patterns using implicit conversion
PersonPattern pattern2 = result;

[AutoPattern]
public record Person(
    string? Name,
    int Age,
    Job Job,
    Address Address,
    HashSet<string> Nicknames,
    Address[] Addresses
);

[AutoPattern]
public record Address(
    string City,
    string Zip
);

[Union]
[AutoPattern]
public partial record Job
{
    public partial record Employed(string Company, string Position);
    public partial record Unemployed;
}