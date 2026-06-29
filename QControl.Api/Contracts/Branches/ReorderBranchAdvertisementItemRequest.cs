namespace Qcontrol.Api.Contracts.Branches;

public sealed class ReorderBranchAdvertisementItemRequest
{
    public int AdvertisementId { get; init; }

    public int DisplayOrder { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
