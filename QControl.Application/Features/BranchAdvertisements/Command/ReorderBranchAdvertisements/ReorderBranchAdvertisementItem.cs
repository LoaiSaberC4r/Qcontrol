namespace Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;

public sealed class ReorderBranchAdvertisementItem
{
    public int AdvertisementId { get; init; }

    public int DisplayOrder { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
