using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchVideos.Shared;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;

public sealed record GetBranchDisplayPlaylistQuery
    : IQuery<BranchDisplayPlaylistResponse>
{
    public int BranchId { get; init; }
}
