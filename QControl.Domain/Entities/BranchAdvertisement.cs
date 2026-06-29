using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchAdvertisement : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public string ImagePath { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public DateTime? DeactivatedOnUtc { get; private set; }

    public Guid? DeactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? DeactivatedByApplicationUser { get; private set; }

    public DateTime? ReactivatedOnUtc { get; private set; }

    public Guid? ReactivatedByApplicationUserId { get; private set; }

    public ApplicationUser? ReactivatedByApplicationUser { get; private set; }

    private BranchAdvertisement()
    {
    }

    public static BranchAdvertisement Create(
        int branchId,
        string imagePath,
        int displayOrder,
        Guid createdByApplicationUserId)
    {
        return new BranchAdvertisement
        {
            BranchId = branchId,
            ImagePath = NormalizePath(imagePath),
            DisplayOrder = NormalizeDisplayOrder(displayOrder),
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void ChangeDisplayOrder(
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        DisplayOrder = NormalizeDisplayOrder(displayOrder);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public bool Deactivate(
        DateTime deactivatedOnUtc,
        Guid deactivatedByApplicationUserId)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = deactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = deactivatedByApplicationUserId;

        return true;
    }

    public bool Reactivate(
        DateTime reactivatedOnUtc,
        Guid reactivatedByApplicationUserId)
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = reactivatedByApplicationUserId;
        LastModifiedByApplicationUserId = reactivatedByApplicationUserId;

        return true;
    }

    private static int NormalizeDisplayOrder(int value)
    {
        if (value is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Display order must be between 1 and 20.");
        }

        return value;
    }

    private static string NormalizePath(string value) =>
        value.Trim().Replace('\\', '/');
}
