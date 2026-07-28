using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class SegmentGlobalizationRequest : AggregateRoot<int>
{
    private SegmentGlobalizationRequest()
    {
    }

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int SegmentId { get; private set; }

    public Segment Segment { get; private set; } = null!;

    public SegmentGlobalizationRequestStatus Status { get; private set; }

    public Guid RequestedByApplicationUserId { get; private set; }

    public ApplicationUser RequestedByApplicationUser { get; private set; } =
        null!;

    public DateTime RequestedOnUtc { get; private set; }

    public Guid? ReviewedByApplicationUserId { get; private set; }

    public ApplicationUser? ReviewedByApplicationUser { get; private set; }

    public DateTime? ReviewedOnUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static SegmentGlobalizationRequest Create(
        int branchId,
        int segmentId,
        Guid requestedByApplicationUserId,
        DateTime requestedOnUtc)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchId));
        }

        if (segmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentId));
        }

        return CreateCore(
            branchId,
            segmentId,
            segment: null,
            requestedByApplicationUserId,
            requestedOnUtc);
    }

    public static SegmentGlobalizationRequest Create(
        int branchId,
        Segment segment,
        Guid requestedByApplicationUserId,
        DateTime requestedOnUtc)
    {
        ArgumentNullException.ThrowIfNull(segment);

        return CreateCore(
            branchId,
            segmentId: 0,
            segment,
            requestedByApplicationUserId,
            requestedOnUtc);
    }

    public void Approve(
        Guid reviewedByApplicationUserId,
        DateTime reviewedOnUtc)
    {
        EnsurePending();
        Status = SegmentGlobalizationRequestStatus.Approved;
        ReviewedByApplicationUserId = reviewedByApplicationUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = null;
    }

    public void Reject(
        Guid reviewedByApplicationUserId,
        DateTime reviewedOnUtc,
        string? rejectionReason)
    {
        EnsurePending();
        Status = SegmentGlobalizationRequestStatus.Rejected;
        ReviewedByApplicationUserId = reviewedByApplicationUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = string.IsNullOrWhiteSpace(rejectionReason)
            ? null
            : rejectionReason.Trim();
    }

    private static SegmentGlobalizationRequest CreateCore(
        int branchId,
        int segmentId,
        Segment? segment,
        Guid requestedByApplicationUserId,
        DateTime requestedOnUtc)
    {
        var request = new SegmentGlobalizationRequest
        {
            BranchId = branchId,
            SegmentId = segmentId,
            Status = SegmentGlobalizationRequestStatus.Pending,
            RequestedByApplicationUserId = requestedByApplicationUserId,
            RequestedOnUtc = requestedOnUtc
        };

        if (segment is not null)
        {
            request.Segment = segment;
        }

        return request;
    }

    private void EnsurePending()
    {
        if (Status != SegmentGlobalizationRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending requests can be reviewed.");
        }
    }
}
