using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class Segment : AggregateRoot<int>
{
    private Segment()
    {
    }

    public string ArabicName { get; private set; } = string.Empty;

    public string EnglishName { get; private set; } = string.Empty;

    public int Priority { get; private set; }

    public SegmentScope Scope { get; private set; }

    public int? OwnerBranchId { get; private set; }

    public Branch? OwnerBranch { get; private set; }

    public bool IsSystemDefault { get; private set; }

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } =
        null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public static Segment CreateGlobal(
        string arabicName,
        string englishName,
        int priority,
        Guid createdByApplicationUserId)
        => CreateCore(
            arabicName,
            englishName,
            priority,
            SegmentScope.Global,
            ownerBranchId: null,
            isSystemDefault: false,
            createdByApplicationUserId);

    public static Segment CreateBranchScoped(
        int ownerBranchId,
        string arabicName,
        string englishName,
        int priority,
        Guid createdByApplicationUserId)
    {
        if (ownerBranchId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ownerBranchId));
        }

        return CreateCore(
            arabicName,
            englishName,
            priority,
            SegmentScope.BranchScoped,
            ownerBranchId,
            isSystemDefault: false,
            createdByApplicationUserId);
    }

    public static Segment CreateSystemDefault(
        string arabicName,
        string englishName,
        Guid createdByApplicationUserId)
        => CreateCore(
            arabicName,
            englishName,
            priority: 0,
            SegmentScope.Global,
            ownerBranchId: null,
            isSystemDefault: true,
            createdByApplicationUserId);

    public void UpdateDefinition(
        string arabicName,
        string englishName,
        int priority,
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc)
    {
        if (priority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        if (IsSystemDefault && priority != 0)
        {
            throw new InvalidOperationException(
                "The system-default segment priority must remain zero.");
        }

        ArabicName = NormalizeRequired(arabicName);
        EnglishName = NormalizeRequired(englishName);
        Priority = priority;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
        ModifiedOnUtc = modifiedOnUtc;
    }

    public void PromoteToGlobal(
        Guid modifiedByApplicationUserId,
        DateTime modifiedOnUtc)
    {
        if (IsSystemDefault)
        {
            throw new InvalidOperationException(
                "The system-default segment cannot change scope.");
        }

        Scope = SegmentScope.Global;
        OwnerBranchId = null;
        LastModifiedByApplicationUserId = modifiedByApplicationUserId;
        ModifiedOnUtc = modifiedOnUtc;
    }

    public void EnsureSystemValues()
    {
        if (!IsSystemDefault)
        {
            throw new InvalidOperationException(
                "Only the system-default segment can be repaired.");
        }

        Scope = SegmentScope.Global;
        OwnerBranchId = null;
        Priority = 0;
    }

    private static Segment CreateCore(
        string arabicName,
        string englishName,
        int priority,
        SegmentScope scope,
        int? ownerBranchId,
        bool isSystemDefault,
        Guid createdByApplicationUserId)
    {
        if (priority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        if ((scope == SegmentScope.Global && ownerBranchId.HasValue) ||
            (scope == SegmentScope.BranchScoped && !ownerBranchId.HasValue))
        {
            throw new ArgumentException(
                "Segment scope and owner branch are inconsistent.");
        }

        if (isSystemDefault &&
            (scope != SegmentScope.Global ||
             ownerBranchId.HasValue ||
             priority != 0))
        {
            throw new ArgumentException(
                "The system-default segment must be global with priority zero.");
        }

        return new Segment
        {
            ArabicName = NormalizeRequired(arabicName),
            EnglishName = NormalizeRequired(englishName),
            Priority = priority,
            Scope = scope,
            OwnerBranchId = ownerBranchId,
            IsSystemDefault = isSystemDefault,
            CreatedByApplicationUserId = createdByApplicationUserId
        };
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A segment name is required.");
        }

        return value.Trim();
    }
}
