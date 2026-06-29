namespace Qcontrol.Application.Features.BranchAdvertisements.Shared;

public sealed class BranchAdvertisementDeleteResponse
{
    public int AdvertisementId { get; init; }

    public int BranchId { get; init; }

    public string Message { get; init; } = string.Empty;
}
