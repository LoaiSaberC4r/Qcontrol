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

    public Guid CreatedByApplicationUserId { get; private set; }

    public ApplicationUser CreatedByApplicationUser { get; private set; } = null!;

    public Guid? LastModifiedByApplicationUserId { get; private set; }

    public ApplicationUser? LastModifiedByApplicationUser { get; private set; }

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
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        int branchId,
        int number,
        string? audioDevice,
        string? controlDevice,
        string? descriptiveName,
        Guid lastModifiedByApplicationUserId)
    {
        BranchId = branchId;
        Number = number;
        AudioDevice = NormalizeOptional(audioDevice);
        ControlDevice = NormalizeOptional(controlDevice);
        DescriptiveName = NormalizeOptional(descriptiveName);
        LastModifiedByApplicationUserId =
            lastModifiedByApplicationUserId;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
