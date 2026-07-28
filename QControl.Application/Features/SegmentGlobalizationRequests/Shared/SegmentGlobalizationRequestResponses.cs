using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;

public class SegmentGlobalizationRequestResponse
{
    public int RequestId { get; init; }
    public SegmentGlobalizationRequestStatus Status { get; init; }
    public int BranchId { get; init; }
    public string BranchArabicName { get; init; } = string.Empty;
    public string BranchEnglishName { get; init; } = string.Empty;
    public int SegmentId { get; init; }
    public string SegmentArabicName { get; init; } = string.Empty;
    public string SegmentEnglishName { get; init; } = string.Empty;
    public SegmentScope SegmentScope { get; init; }
    public int? SegmentOwnerBranchId { get; init; }
    public int SegmentPriority { get; init; }
    public Guid RequestedByApplicationUserId { get; init; }
    public string RequestedByName { get; init; } = string.Empty;
    public DateTime RequestedOnUtc { get; init; }
    public Guid? ReviewedByApplicationUserId { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime? ReviewedOnUtc { get; init; }
    public string? RejectionReason { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public string? Message { get; init; }
}
