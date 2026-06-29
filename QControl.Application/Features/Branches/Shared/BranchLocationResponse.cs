namespace Qcontrol.Application.Features.Branches.Shared;

public sealed record BranchLocationResponse
{
    public int Id { get; init; }

    public string Governorate { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }
}
