using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchVideos.Shared;

namespace Qcontrol.Application.Features.BranchVideos.Command.DeactivateBranchVideo;

public sealed record DeactivateBranchVideoCommand
    : ICommand<BranchVideoStateResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public int VideoId { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public IEnumerable<string> Tags => BranchVideoCacheTags.ForBranch(BranchId);
}
