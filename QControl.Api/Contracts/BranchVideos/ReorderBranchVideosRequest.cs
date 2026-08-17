namespace Qcontrol.Api.Contracts.BranchVideos;

public sealed class ReorderBranchVideosRequest
{
    public List<ReorderBranchVideoItemRequest> Items { get; init; } = new();
}
