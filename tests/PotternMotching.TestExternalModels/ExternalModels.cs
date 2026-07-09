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

public record ExternalGenericBox<T>(
    T Value);

public record ExternalGenericEnvelope<T>(
    string Id,
    ExternalGenericBox<T> Box);

// Nullable collection test models — reproduce nullable collection issues.

/// <summary>
/// Reproduces Issue 1 &amp; 2: nullable collection (the collection itself can be null)
/// and implicit conversion crash when .From() is called on null.
/// </summary>
public record ExternalNullableCollection(
    string Id,
    List<string>? Names,
    Dictionary<string, int>? Scores);

/// <summary>
/// Reproduces Issue 2: nullable elements inside a collection (<c>List&lt;string?&gt;</c>).
/// The generated element pattern should use <c>ValuePattern&lt;string?&gt;</c>,
/// not <c>ValuePattern&lt;string&gt;</c>.
/// </summary>
public record ExternalNullableElements(
    string Id,
    List<string?> Tags);

/// <summary>
/// Reproduces Issue 3: <c>HashSet</c> with <c>SetPatternDefault</c> — null inner matcher
/// when using <c>default</c>.
/// </summary>
public record ExternalNullableSet(
    string Id,
    HashSet<string>? Flags);
