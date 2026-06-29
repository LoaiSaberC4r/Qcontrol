namespace Qcontrol.Api.Contracts.Branches;

public sealed class BranchLocationRequest
{
    public string Governorate { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }
}
