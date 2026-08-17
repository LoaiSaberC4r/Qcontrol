using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

internal static class BranchVideoResponseFactory
{
    public static BranchVideoResponse FromEntity(BranchVideo video) =>
        new()
        {
            Id = video.Id,
            BranchId = video.BranchId,
            OriginalFileName = video.OriginalFileName,
            DisplayOrder = video.DisplayOrder,
            IsActive = video.IsActive,
            ProcessingStatus = video.ProcessingStatus,
            StreamUrl = video.ProcessingStatus == BranchVideoProcessingStatus.Ready
                ? BranchMediaUrlMapper.ToMediaUrl(video.HlsManifestPath)
                : null,
            RowVersion = RowVersionConverter.ToBase64(video.RowVersion)
        };

    public static BranchVideoStateResponse StateFromEntity(
        BranchVideo video,
        string message) =>
        new()
        {
            VideoId = video.Id,
            BranchId = video.BranchId,
            DisplayOrder = video.DisplayOrder,
            IsActive = video.IsActive,
            ProcessingStatus = video.ProcessingStatus,
            RowVersion = RowVersionConverter.ToBase64(video.RowVersion),
            Message = message
        };
}
