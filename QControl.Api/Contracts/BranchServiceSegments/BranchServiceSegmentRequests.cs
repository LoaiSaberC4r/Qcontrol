namespace Qcontrol.Api.Contracts.BranchServiceSegments;

public sealed class AssignBranchServiceSegmentsRequest
{
    public IReadOnlyList<AssignBranchServiceSegmentItemRequest> Items
    {
        get;
        init;
    } = Array.Empty<AssignBranchServiceSegmentItemRequest>();
}

public sealed class AssignBranchServiceSegmentItemRequest
{
    public int SegmentId { get; init; }
    public int Quota { get; init; }
}

public sealed class UpdateBranchServiceSegmentQuotaRequest
{
    public int Quota { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}
