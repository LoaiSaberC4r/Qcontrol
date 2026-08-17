using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchVideos.Shared;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchDisplayPlaylist;

internal sealed class GetBranchDisplayPlaylistSpec
    : Specification<BranchVideo, BranchDisplayPlaylistVideoResponse>
{
    public GetBranchDisplayPlaylistSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        AddCriteria(x => x.IsActive);
        AddCriteria(x => x.ProcessingStatus == BranchVideoProcessingStatus.Ready);
        AddCriteria(x => x.HlsManifestPath != null);
        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);
        Select(x => new BranchDisplayPlaylistVideoResponse
        {
            VideoId = x.Id,
            DisplayOrder = x.DisplayOrder,
            StreamUrl = "/Media/" + x.HlsManifestPath!
        });
    }
}
