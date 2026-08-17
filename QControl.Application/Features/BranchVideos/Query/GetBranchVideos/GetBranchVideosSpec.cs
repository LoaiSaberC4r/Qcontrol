using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchVideos.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchVideos.Query.GetBranchVideos;

internal sealed class GetBranchVideosSpec
    : Specification<BranchVideo, BranchVideoResponse>
{
    public GetBranchVideosSpec(int branchId, bool? isActive)
    {
        AddCriteria(x => x.BranchId == branchId);
        if (isActive.HasValue)
        {
            AddCriteria(x => x.IsActive == isActive.Value);
        }

        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);
        Select(x => new BranchVideoResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            OriginalFileName = x.OriginalFileName,
            DisplayOrder = x.DisplayOrder,
            IsActive = x.IsActive,
            ProcessingStatus = x.ProcessingStatus,
            StreamUrl = x.ProcessingStatus == BranchVideoProcessingStatus.Ready &&
                        x.HlsManifestPath != null
                ? "/Media/" + x.HlsManifestPath
                : null,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
