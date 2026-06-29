namespace Qcontrol.Application.Features.BranchAdvertisements.Shared;

public sealed class BranchAdvertisementResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
