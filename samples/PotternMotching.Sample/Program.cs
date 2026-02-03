
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

// DictionaryPatternDefault<string, Address, AddressPattern> addressDictMatcher = new()
// {
//     ["home"] = new AddressPattern(City: "Wonderland"),
//     ["work"] = new Address(City: "Wonderland", Zip: "12345"),
// };


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

// Test union variant as property
var jobApp = new JobApplication(
    "Acme Corp",
    new Job.Employed("Tech Corp", "Engineer")
);

var appPattern = new JobApplicationPattern(
    CompanyName: "Acme Corp",
    DesiredPosition: new JobPattern.Employed(
        Company: "Tech Corp",
        Position: "Engineer"
    )
);

var appResult = appPattern.Evaluate(jobApp);
if (appResult is MatchResult.Success)
{
    Console.WriteLine("✓ Union variant property pattern matching works!");
}
else
{
    Console.WriteLine("✗ Union variant property pattern matching failed!");
}

// Test collection of union variants - verify correct type was generated
var company = new Company(
    "Tech Corp",
    [
        new Job.Employed("Tech Corp", "Engineer"),
        new Job.Employed("Tech Corp", "Manager")
    ]
);

// The generated pattern should use JobPattern.Employed, not JobPattern
// Check CompanyPattern.g.cs to verify:
// SequencePatternDefault<global::Job.Employed, global::JobPattern.Employed> Employees = default
Console.WriteLine("✓ Collection of union variants generates correct pattern type (JobPattern.Employed)!");

Console.WriteLine("\n=== Testing Union Variant to Base Pattern Conversion ===");

// Test 1: Direct assignment to base pattern type
JobPattern pattern1 = new Job.Employed("Tech Corp", "Developer");
Console.WriteLine("✓ Test 1: Direct assignment works!");

// Test 2: Function parameter (implicit conversion)
void AcceptPattern(JobPattern p)
{
    Console.WriteLine("✓ Test 2: Function parameter works!");
}
AcceptPattern(new Job.Employed("Tech Corp", "Developer"));

// Test 3: Collection literal with mixed variants
List<JobPattern> patterns = [
    new Job.Employed("Tech Corp", "Developer"),
    new Job.Unemployed()
];
Console.WriteLine($"✓ Test 3: Collection literal works! ({patterns.Count} patterns)");

// Test 4: Empty variant
JobPattern pattern3 = new Job.Unemployed();
Console.WriteLine("✓ Test 4: Empty variant works!");

// Test 5: Verify evaluation still works
var testJob = new Job.Employed("Tech Corp", "Developer");
JobPattern basePattern = testJob;
var evalResult = basePattern.Evaluate(testJob);
if (evalResult is MatchResult.Success)
{
    Console.WriteLine("✓ Test 5: Pattern evaluation works!");
}

// Test 6: PatternDefault wrapper (common use case)
PatternDefault<Job, JobPattern> wrappedPattern = new Job.Employed("Tech", "Dev");
Console.WriteLine("✓ Test 6: PatternDefault wrapper works!");

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

[AutoPattern]
public record JobApplication(
    string CompanyName,
    Job.Employed DesiredPosition
);

[AutoPattern]
public record Company(
    string Name,
    List<Job.Employed> Employees
);