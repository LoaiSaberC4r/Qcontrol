using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;

namespace QControl.Domain.Entities;

public sealed class Window : AggregateRoot<int>
{
    private readonly List<Terminal> _terminals = new();
    private readonly List<DisplayWindow> _displayWindows = new();

    public string? DescriptiveName { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public int BranchId { get; private set; }

    public int WaitingAreaId { get; private set; }

    public WaitingArea WaitingArea { get; private set; } = null!;

    public string? IPAddress { get; private set; }

    public bool EnableTicketBooking { get; private set; }

    public bool EnableDirectCall { get; private set; }

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

    public IReadOnlyCollection<Terminal> Terminals =>
        _terminals.AsReadOnly();

    public IReadOnlyCollection<DisplayWindow> DisplayWindows =>
        _displayWindows.AsReadOnly();

    private Window()
    {
    }

    public static Window Create(
        int branchId,
        int waitingAreaId,
        string number,
        string? descriptiveName,
        string? ipAddress,
        bool enableTicketBooking,
        bool enableDirectCall,
        Guid createdByApplicationUserId)
    {
        return new Window
        {
            BranchId = branchId,
            WaitingAreaId = waitingAreaId,
            Number = number.Trim(),
            DescriptiveName = NormalizeOptional(descriptiveName),
            IPAddress = NormalizeOptional(ipAddress),
            EnableTicketBooking = enableTicketBooking,
            EnableDirectCall = enableDirectCall,
            IsActive = true,
            CreatedByApplicationUserId = createdByApplicationUserId,
            LastModifiedByApplicationUserId = null
        };
    }

    public void Update(
        string number,
        string? descriptiveName,
        string? ipAddress,
        bool enableTicketBooking,
        bool enableDirectCall,
        Guid lastModifiedByApplicationUserId)
    {
        Number = number.Trim();
        DescriptiveName = NormalizeOptional(descriptiveName);
        IPAddress = NormalizeOptional(ipAddress);
        EnableTicketBooking = enableTicketBooking;
        EnableDirectCall = enableDirectCall;
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
