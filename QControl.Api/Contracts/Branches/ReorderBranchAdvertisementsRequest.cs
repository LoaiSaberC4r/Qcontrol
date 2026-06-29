namespace Qcontrol.Api.Contracts.Branches;

public sealed class ReorderBranchAdvertisementsRequest
{
    public List<ReorderBranchAdvertisementItemRequest> Items { get; init; } = new();
}
