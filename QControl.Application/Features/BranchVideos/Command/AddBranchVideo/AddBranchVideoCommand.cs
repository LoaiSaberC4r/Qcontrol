using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Qcontrol.Application.Features.BranchVideos.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchVideos.Command.AddBranchVideo;

public sealed record AddBranchVideoCommand
    : ICommand<BranchVideoResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public IFormFile? Video { get; init; }
    public int DisplayOrder { get; init; }

    public IEnumerable<string> Tags => BranchVideoCacheTags.ForBranch(BranchId);
}
