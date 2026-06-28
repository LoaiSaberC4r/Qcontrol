namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

public sealed record DisplayLinkedWindowResponse
{
    public int Id { get; init; }

    public int WaitingAreaId { get; init; }

    public int WaitingAreaNumber { get; init; }

    public string? WaitingAreaDescriptiveName { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public bool IsDeleted { get; init; }
}
