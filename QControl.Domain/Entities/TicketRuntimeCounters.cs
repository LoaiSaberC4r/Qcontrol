using BuildingBlock.Domain.EntitiesHelper;

namespace QControl.Domain.Entities;

public sealed class TicketNumberSequence : Entity<int>
{
    private TicketNumberSequence() { }
    public int BranchId { get; private set; }
    public int ServiceId { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public int LastIssuedNumber { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static TicketNumberSequence Create(int branchId, int serviceId,
        DateOnly businessDate, int firstIssuedNumber, DateTime createdOnUtc) => new()
        {
            BranchId = branchId,
            ServiceId = serviceId,
            BusinessDate = businessDate,
            LastIssuedNumber = firstIssuedNumber,
            CreatedOnUtc = createdOnUtc
        };

    public bool TryAllocate(int rangeEnd, DateTime modifiedOnUtc, out int number)
    {
        if (LastIssuedNumber >= rangeEnd)
        {
            number = default;
            return false;
        }
        number = ++LastIssuedNumber;
        ModifiedOnUtc = modifiedOnUtc;
        return true;
    }
}

public sealed class BranchServiceSegmentDailyUsage : Entity<int>
{
    private BranchServiceSegmentDailyUsage() { }
    public int BranchServiceSegmentId { get; private set; }
    public BranchServiceSegment BranchServiceSegment { get; private set; } = null!;
    public DateOnly BusinessDate { get; private set; }
    public int ConsumedCount { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static BranchServiceSegmentDailyUsage Create(int relationshipId,
        DateOnly businessDate, int consumedCount, DateTime createdOnUtc) => new()
        {
            BranchServiceSegmentId = relationshipId,
            BusinessDate = businessDate,
            ConsumedCount = consumedCount,
            CreatedOnUtc = createdOnUtc
        };

    public bool TryConsume(int quota, DateTime modifiedOnUtc)
    {
        if (ConsumedCount >= quota) return false;
        ConsumedCount++;
        ModifiedOnUtc = modifiedOnUtc;
        return true;
    }

    public void Release(DateTime modifiedOnUtc)
    {
        if (ConsumedCount > 0) ConsumedCount--;
        ModifiedOnUtc = modifiedOnUtc;
    }
}
