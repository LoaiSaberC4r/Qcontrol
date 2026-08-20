using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchVideos.Shared;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayRuntimePlaylist;

public sealed record GetBranchDisplayRuntimePlaylistQuery
    : IQuery<BranchDisplayPlaylistResponse>
{
    public int BranchId { get; init; }
}
