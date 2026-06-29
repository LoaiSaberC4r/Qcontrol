namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

public sealed record UpdateWindowResponse
{
    public int Id { get; init; }

    public int WaitingAreaId { get; init; }

    public int BranchId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
