using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchVideos.Shared;

public sealed class BranchVideoStateResponse
{
    public int VideoId { get; init; }
    public int BranchId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public BranchVideoProcessingStatus ProcessingStatus { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
