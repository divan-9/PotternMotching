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

public class ExternalNonRecord
{
    public string Id { get; set; } = string.Empty;
}
