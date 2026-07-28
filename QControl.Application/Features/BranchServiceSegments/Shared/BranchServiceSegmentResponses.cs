using BuildingBlock.Domain.SharedDto;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceSegments.Shared;

public sealed class AssignedBranchServiceSegmentItemResponse
{
    public int BranchServiceSegmentId { get; init; }
    public int SegmentId { get; init; }
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public SegmentScope Scope { get; init; }
    public int? OwnerBranchId { get; init; }
    public int Priority { get; init; }
    public int Quota { get; init; }
    public bool IsSystemDefault { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

public sealed class AssignedBranchServiceSegmentsResponse
{
    public int BranchId { get; init; }
    public int LeafServiceId { get; init; }
    public string? RangePrefix { get; init; }
    public int RangeStartNumber { get; init; }
    public int RangeEndNumber { get; init; }
    public int Capacity { get; init; }
    public int AllocatedNonDefaultQuota { get; init; }
    public int RemainingQuota { get; init; }
    public IReadOnlyList<AssignedBranchServiceSegmentItemResponse> Segments
    {
        get;
        init;
    } = Array.Empty<AssignedBranchServiceSegmentItemResponse>();
    public string? Message { get; init; }
}

public sealed class AvailableBranchServiceSegmentResponse
{
    public int SegmentId { get; init; }
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public SegmentScope Scope { get; init; }
    public int? OwnerBranchId { get; init; }
    public int Priority { get; init; }
}

public sealed class AvailableBranchServiceSegmentsResponse
{
    public int BranchId { get; init; }
    public int LeafServiceId { get; init; }
    public int RemainingQuota { get; init; }
    public Pagination<AvailableBranchServiceSegmentResponse> Segments
    {
        get;
        init;
    } = null!;
}

public sealed class UnassignBranchServiceSegmentResponse
{
    public int BranchId { get; init; }
    public int LeafServiceId { get; init; }
    public int SegmentId { get; init; }
    public int RemovedQuota { get; init; }
    public int UpdatedDefaultQuota { get; init; }
    public string Message { get; init; } = string.Empty;
}
