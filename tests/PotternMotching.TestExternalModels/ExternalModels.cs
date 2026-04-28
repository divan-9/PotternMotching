namespace PotternMotching.TestExternalModels;

using Dunet;

public record ExternalAddress(
    string City,
    string Zip);

public record ExternalUserDto(
    string Id,
    string Name,
    string[] Roles,
    ExternalAddress Address);

public record ExternalCollectionsDto(
    string[] Roles,
    List<int> Numbers,
    HashSet<string> Flags,
    Dictionary<string, int> Scores);

public record ExternalUnknown(
    string Value);

public record ExternalWrappedUnknown(
    string Id,
    ExternalUnknown Unknown);

public class ExternalEntityBase
{
    public string Id { get; init; } = string.Empty;
}

public class ExternalClassDto : ExternalEntityBase
{
    public string Name { get; init; } = string.Empty;
    public string[] Roles { get; init; } = [];
    public ExternalAddress Address { get; init; } = new(string.Empty, string.Empty);
    public int RoleCount => Roles.Length;
    public string Hidden { private get; init; } = string.Empty;
    internal string InternalOnly { get; init; } = string.Empty;
}

[Union]
public partial record ExternalJob
{
    public partial record Employed(string Company, string Position);
    public partial record Unemployed;
}

public record ExternalJobApplication(
    string CompanyName,
    ExternalJob DesiredPosition);

public record ExternalCompany(
    string Name,
    List<ExternalJob.Employed> Employees);

[Union]
public partial record ExternalContent
{
    public partial record String(string Value);
    public partial record Object(string Id);
    public partial record Composite;
}

public struct ExternalNonRecord
{
    public string Id { get; set; }
}
