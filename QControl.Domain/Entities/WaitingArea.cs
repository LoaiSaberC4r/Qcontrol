using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class WaitingArea : AggregateRoot<int>
{
    private readonly List<Window> _windows = new();

    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public int Number { get; private set; }

    public string? AudioDevice { get; private set; }

    public string? ControlDevice { get; private set; }

    public string? DescriptiveName { get; private set; }

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

    public IReadOnlyCollection<Window> Windows =>
        _windows.AsReadOnly();

    private WaitingArea()
    {
    }

    public static WaitingArea Create(
        int branchId,
        int number,
        string? audioDevice,
        string? controlDevice,
        string? descriptiveName,
        Guid createdByApplicationUserId)
    {
        return new WaitingArea
        {
            BranchId = branchId,
            Number = number,
            AudioDevice = NormalizeOptional(audioDevice),
            ControlDevice = NormalizeOptional(controlDevice),
            DescriptiveName = NormalizeOptional(descriptiveName),
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        int number,
        string? audioDevice,
        string? controlDevice,
        string? descriptiveName,
        Guid lastModifiedByApplicationUserId)
    {
        Number = number;
        AudioDevice = NormalizeOptional(audioDevice);
        ControlDevice = NormalizeOptional(controlDevice);
        DescriptiveName = NormalizeOptional(descriptiveName);
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    public void Deactivate(
        DateTime deactivatedOnUtc,
        Guid deactivatedByApplicationUserId)
    {
        IsActive = false;
        DeactivatedOnUtc = deactivatedOnUtc;
        DeactivatedByApplicationUserId = deactivatedByApplicationUserId;
        LastModifiedByApplicationUserId =
            deactivatedByApplicationUserId;
    }

    public void Reactivate(
        DateTime reactivatedOnUtc,
        Guid reactivatedByApplicationUserId)
    {
        IsActive = true;
        ReactivatedOnUtc = reactivatedOnUtc;
        ReactivatedByApplicationUserId = reactivatedByApplicationUserId;
        LastModifiedByApplicationUserId =
            reactivatedByApplicationUserId;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
