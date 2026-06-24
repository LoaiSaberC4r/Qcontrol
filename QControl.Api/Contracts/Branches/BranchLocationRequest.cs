namespace Qcontrol.Api.Contracts.Branches;

public sealed class BranchLocationRequest
{
    public string? Governorate { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Address { get; init; }

    public string? Longitude { get; init; }

    public string? Latitude { get; init; }
}