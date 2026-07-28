using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Segments.Shared;

public class SegmentResponse
{
    public int Id { get; init; }
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public SegmentScope Scope { get; init; }
    public int? OwnerBranchId { get; init; }
    public string? OwnerBranchArabicName { get; init; }
    public string? OwnerBranchEnglishName { get; init; }
    public bool IsSystemDefault { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public DateTime CreatedOnUtc { get; init; }
    public DateTime? ModifiedOnUtc { get; init; }
    public string? Message { get; init; }
}

public sealed class SegmentDetailsResponse : SegmentResponse
{
    public int AssignedLeafServicesCount { get; init; }
    public int? PendingGlobalizationRequestId { get; init; }
}

public sealed class CreateBranchSegmentResponse : SegmentResponse
{
    public int GlobalizationRequestId { get; init; }
    public SegmentGlobalizationRequestStatus GlobalizationRequestStatus
    {
        get;
        init;
    }
}
