namespace PotternMotching.TestExternalModels;

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

public struct ExternalNonRecord
{
    public string Id { get; set; }
}
