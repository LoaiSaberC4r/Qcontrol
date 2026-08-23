using BuildingBlock.Domain.EntitiesHelper;

namespace QControl.Domain.Entities;

public sealed class BranchConfiguration : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public TimeSpan AllowedTime { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private BranchConfiguration()
    {
    }

    public static BranchConfiguration Create(
        int branchId,
        TimeSpan allowedTime)
    {
        ValidateBranchId(branchId);
        ValidateAllowedTime(allowedTime);

        return new BranchConfiguration
        {
            BranchId = branchId,
            AllowedTime = allowedTime
        };
    }

    public void UpdateAllowedTime(TimeSpan allowedTime)
    {
        ValidateAllowedTime(allowedTime);
        AllowedTime = allowedTime;
    }

    private static void ValidateBranchId(int branchId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchId));
        }
    }

    private static void ValidateAllowedTime(TimeSpan allowedTime)
    {
        if (allowedTime < TimeSpan.Zero ||
            allowedTime >= TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(allowedTime));
        }
    }
}
