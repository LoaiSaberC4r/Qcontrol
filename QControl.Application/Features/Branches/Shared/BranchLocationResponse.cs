namespace Qcontrol.Application.Features.Branches.Shared;

public sealed record BranchLocationResponse
{
    public int Id { get; init; }

    public string? Governorate { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Address { get; init; }

    public string? Longitude { get; init; }

    public string? Latitude { get; init; }
}