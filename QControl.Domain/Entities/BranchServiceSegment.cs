using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchServiceSegment : Entity<int>
{
    private BranchServiceSegment()
    {
    }

    public int BranchServiceId { get; private set; }

    public BranchService BranchService { get; private set; } = null!;

    public int SegmentId { get; private set; }

    public Segment Segment { get; private set; } = null!;

    public int Quota { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } =
        null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static BranchServiceSegment Create(
        int branchServiceId,
        int segmentId,
        int quota,
        Guid createdByApplicationUserId)
    {
        if (branchServiceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchServiceId));
        }

        if (segmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentId));
        }

        EnsureQuota(quota);

        return new BranchServiceSegment
        {
            BranchServiceId = branchServiceId,
            SegmentId = segmentId,
            Quota = quota,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public static BranchServiceSegment Create(
        BranchService branchService,
        Segment segment,
        int quota,
        Guid createdByApplicationUserId)
    {
        ArgumentNullException.ThrowIfNull(branchService);
        ArgumentNullException.ThrowIfNull(segment);
        EnsureQuota(quota);

        return new BranchServiceSegment
        {
            BranchService = branchService,
            Segment = segment,
            Quota = quota,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public void UpdateQuota(
        int quota,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc)
    {
        EnsureQuota(quota);
        Quota = quota;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
        ModifiedOnUtc = modifiedOnUtc;
    }

    private static void EnsureQuota(int quota)
    {
        if (quota < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quota));
        }
    }
}
