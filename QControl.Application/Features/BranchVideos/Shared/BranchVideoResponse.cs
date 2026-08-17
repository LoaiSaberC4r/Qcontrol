using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

public sealed class BranchVideoResponse
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public BranchVideoProcessingStatus ProcessingStatus { get; init; }
    public string? StreamUrl { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}
