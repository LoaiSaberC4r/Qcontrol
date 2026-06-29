namespace Qcontrol.Application.Features.BranchAdvertisements.Shared;

public sealed class BranchAdvertisementStateResponse
{
    public int AdvertisementId { get; init; }

    public int BranchId { get; init; }

    public bool IsActive { get; init; }

    public int DisplayOrder { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
