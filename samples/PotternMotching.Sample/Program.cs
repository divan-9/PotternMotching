
using Dunet;
using PotternMotching;
using PotternMotching.Matchers;

var result = new Person(
    Name: "Alice",
    Age: 30,
    Nicknames: ["Ally", "Lice"],
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

var pattern = new PersonPattern(
    Name: "Alice",
    Age: 30,
    Job: new JobPattern.Employed(
        Company: "Tech Corp"),
    Nicknames: CollectionMatcher.AnyOrder([
        "Ally",
        "Lice"
    ]),
    Addresses: CollectionMatcher.Sequence([
        new AddressPattern(
            City: "Wonderland"),
        new AddressPattern(
            City: "Looking Glass",
            Zip: "67890"),
    ]));

public record Person(
    string? Name,
    int Age,
    Job Job,
    HashSet<string> Nicknames,
    Address[] Addresses
);

[AutoPattern]
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

public abstract record JobPattern : IMatcher<Job>
{
    public MatchResult Evaluate(
        Job value,
        string path = "")
    {
        throw new NotImplementedException();
    }

    public static implicit operator DefaultMatcher<Job>(
        JobPattern matcher)
    {
        return new DefaultMatcher<Job>(matcher);
    }

    public record Employed(
        DefaultMatcher<string> Company = default,
        DefaultMatcher<string> Position = default) : JobPattern;

    public record Unemployed() : JobPattern;
};

public record PersonPattern(
    DefaultMatcher<string?> Name = default,
    DefaultMatcher<int> Age = default,
    DefaultMatcher<Job> Job = default,
    DefaultMatcher<IEnumerable<string>> Nicknames = default,
    DefaultMatcher<IEnumerable<Address>> Addresses = default);

public record AddressPattern(
    DefaultMatcher<string> City = default,
    DefaultMatcher<string> Zip = default) : IMatcher<Address>
{
    public MatchResult Evaluate(
        Address value,
        string path = "")
    {

    }
}