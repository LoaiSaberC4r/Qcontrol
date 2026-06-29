namespace Qcontrol.Application.Features.Windows.Query.GetWindows;

public sealed record WindowListItemResponse
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public int WaitingAreaId { get; init; }

    public int WaitingAreaNumber { get; init; }

    public string? WaitingAreaDescriptiveName { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
