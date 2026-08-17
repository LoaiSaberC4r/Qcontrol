using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchVideos.Shared;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchVideos;

public sealed record GetBranchVideosQuery
    : IQuery<IReadOnlyList<BranchVideoResponse>>
{
    public int BranchId { get; init; }
    public bool? IsActive { get; init; }
}
