using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchVideos.Shared;

namespace Qcontrol.Application.Features.BranchVideos.Command.ReorderBranchVideos;

public sealed record ReorderBranchVideosCommand
    : ICommand<IReadOnlyList<BranchVideoResponse>>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public List<ReorderBranchVideoItem> Items { get; init; } = new();
    public IEnumerable<string> Tags => BranchVideoCacheTags.ForBranch(BranchId);
}
