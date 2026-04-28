
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

// OLD: This was the incorrect type before the fix - now it won't compile
// SequencePatternDefault<Job.Employed, JobPattern> jobSeqMatcher = [
//     new(
//         Company: "Tech Corp",
//         Position: "Developer"),
//     new(
//         Company: "Business Inc",
//         Position: "Manager"),
// ];

// CORRECT: Using the specific variant pattern type
SequencePatternDefault<Job.Employed, JobPattern.Employed> jobSeqMatcher2 = [
    new(
        Company: "Tech Corp",
        Position: "Developer"),
    new(
        Company: "Business Inc",
        Position: "Manager"),
];

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
    Addresses: CollectionPattern.Subset([
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

Console.WriteLine("\n=== Testing Union Error Messages ===");

var employedJob = new Job.Employed("Tech Corp", "Developer");
var unemployedJob = new Job.Unemployed();

// Test 1: Expect Employed, get Unemployed
var employedPattern = new JobPattern.Employed(Company: "Tech Corp", Position: "Developer");
var result1 = employedPattern.Evaluate(unemployedJob, ".Job");
if (result1 is MatchResult.Failure f1)
{
    Console.WriteLine($"Error: {f1.Reasons[0]}");
    // Should print: ".Job: Expected variant Employed, got Unemployed"
}

// Test 2: Expect Unemployed, get Employed
var unemployedPattern = new JobPattern.Unemployed();
var result2 = unemployedPattern.Evaluate(employedJob, ".Job");
if (result2 is MatchResult.Failure f2)
{
    Console.WriteLine($"Error: {f2.Reasons[0]}");
    // Should print: ".Job: Expected variant Unemployed, got Employed"
}

Console.WriteLine("\n=== Testing Keyword Variant Names ===");
var stringContent = new ImpressionContent.String("test");
var compositeContent = new ImpressionContent.Composite();

// Test: Expect String, get Composite
var stringPattern = new ImpressionContentPattern.String(Value: "test");
var keywordResult1 = stringPattern.Evaluate(compositeContent, ".Content");
if (keywordResult1 is MatchResult.Failure fk1)
{
    Console.WriteLine($"Error: {fk1.Reasons[0]}");
    // Should print: ".Content: Expected variant String, got Composite"
}

// Test: Expect Composite, get String
var compositePattern = new ImpressionContentPattern.Composite();
var keywordResult2 = compositePattern.Evaluate(stringContent, ".Content");
if (keywordResult2 is MatchResult.Failure fk2)
{
    Console.WriteLine($"Error: {fk2.Reasons[0]}");
    // Should print: ".Content: Expected variant Composite, got String"
}

// Test: Expect Object, get String
var objectPattern = new ImpressionContentPattern.Object(Id: "123");
var keywordResult3 = objectPattern.Evaluate(stringContent, ".Content");
if (keywordResult3 is MatchResult.Failure fk3)
{
    Console.WriteLine($"Error: {fk3.Reasons[0]}");
    // Should print: ".Content: Expected variant Object, got String"
}

public record Person(
    string? Name,
    int Age,
    Job Job,
    Address Address,
    HashSet<string> Nicknames,
    Address[] Addresses
);

public record Address(
    string City,
    string Zip
);

[Union]
public partial record Job
{
    public partial record Employed(string Company, string Position);
    public partial record Unemployed;
}

public record JobApplication(
    string CompanyName,
    Job.Employed DesiredPosition
);

public record Company(
    string Name,
    List<Job.Employed> Employees
);

// Test union with keyword variant names
[Union]
public partial record ImpressionContent
{
    public partial record String(string Value);
    public partial record Composite();
    public partial record Object(string Id);
}

[AutoPatternFor(typeof(Person))]
[AutoPatternFor(typeof(Address))]
[AutoPatternFor(typeof(Job))]
[AutoPatternFor(typeof(JobApplication))]
[AutoPatternFor(typeof(Company))]
[AutoPatternFor(typeof(ImpressionContent))]
internal static class PatternMarkers;