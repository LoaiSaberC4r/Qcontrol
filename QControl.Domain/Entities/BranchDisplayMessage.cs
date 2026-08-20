using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class BranchDisplayMessage : Entity<int>
{
    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;
    public string TextAr { get; private set; } = string.Empty;
    public string TextEn { get; private set; } = string.Empty;
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

    private BranchDisplayMessage()
    {
    }

    public static BranchDisplayMessage Create(
        int branchId,
        string textAr,
        string textEn,
        int displayOrder,
        Guid createdByApplicationUserId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(branchId));
        }

        if (createdByApplicationUserId == Guid.Empty)
        {
            throw new ArgumentException("A creator is required.", nameof(createdByApplicationUserId));
        }

        return new BranchDisplayMessage
        {
            BranchId = branchId,
            TextAr = NormalizeRequired(textAr),
            TextEn = NormalizeRequired(textEn),
            DisplayOrder = NormalizeDisplayOrder(displayOrder),
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    public void Update(
        string textAr,
        string textEn,
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        TextAr = NormalizeRequired(textAr);
        TextEn = NormalizeRequired(textEn);
        DisplayOrder = NormalizeDisplayOrder(displayOrder);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public void ChangeDisplayOrder(
        int displayOrder,
        Guid lastModifiedByApplicationUserId)
    {
        DisplayOrder = NormalizeDisplayOrder(displayOrder);
        LastModifiedByApplicationUserId = lastModifiedByApplicationUserId;
    }

    public bool Deactivate(DateTime deactivatedOnUtc, Guid applicationUserId)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = applicationUserId;
        LastModifiedByApplicationUserId = applicationUserId;
        return true;
    }

    public bool Reactivate(DateTime reactivatedOnUtc, Guid applicationUserId)
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = applicationUserId;
        LastModifiedByApplicationUserId = applicationUserId;
        return true;
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", nameof(value));
        }

        return value.Trim();
    }

    private static int NormalizeDisplayOrder(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        return value;
    }
}
