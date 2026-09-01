using BuildingBlock.Domain.EntitiesHelper;

namespace QControl.Domain.Entities;

public sealed class BranchConfiguration : Entity<int>
{
    public int BranchId { get; private set; }

    public Branch Branch { get; private set; } = null!;

    public TimeSpan AllowedTime { get; private set; }

    /// <summary>Maximum attempts in one Ticket call cycle.</summary>
    public int MaximumTicketCallAttempts { get; private set; } = 3;

    /// <summary>Minutes a NoShow Ticket remains active before system cancellation.</summary>
    public int TicketNoShowAutoCancellationMinutes { get; private set; } = 30;

    /// <summary>Days a terminal Ticket remains in operational storage.</summary>
    public int TicketArchiveRetentionDays { get; private set; } = 30;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private BranchConfiguration()
    {
    }

    public static BranchConfiguration Create(
        int branchId,
        TimeSpan allowedTime,
        int maximumTicketCallAttempts = 3,
        int ticketNoShowAutoCancellationMinutes = 30,
        int ticketArchiveRetentionDays = 30)
    {
        ValidateBranchId(branchId);
        ValidateAllowedTime(allowedTime);
        ValidateTicketSettings(maximumTicketCallAttempts,
            ticketNoShowAutoCancellationMinutes, ticketArchiveRetentionDays);

        return new BranchConfiguration
        {
            BranchId = branchId,
            AllowedTime = allowedTime,
            MaximumTicketCallAttempts = maximumTicketCallAttempts,
            TicketNoShowAutoCancellationMinutes = ticketNoShowAutoCancellationMinutes,
            TicketArchiveRetentionDays = ticketArchiveRetentionDays
        };
    }

    public void UpdateAllowedTime(TimeSpan allowedTime)
    {
        ValidateAllowedTime(allowedTime);
        AllowedTime = allowedTime;
    }

    public void UpdateTicketSettings(
        int maximumTicketCallAttempts,
        int ticketNoShowAutoCancellationMinutes,
        int ticketArchiveRetentionDays)
    {
        ValidateTicketSettings(maximumTicketCallAttempts,
            ticketNoShowAutoCancellationMinutes, ticketArchiveRetentionDays);
        MaximumTicketCallAttempts = maximumTicketCallAttempts;
        TicketNoShowAutoCancellationMinutes = ticketNoShowAutoCancellationMinutes;
        TicketArchiveRetentionDays = ticketArchiveRetentionDays;
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

    private static void ValidateTicketSettings(int maximumAttempts,
        int noShowTimeoutMinutes, int archiveRetentionDays)
    {
        if (maximumAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        if (noShowTimeoutMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(noShowTimeoutMinutes));
        if (archiveRetentionDays <= 0) throw new ArgumentOutOfRangeException(nameof(archiveRetentionDays));
    }
}
