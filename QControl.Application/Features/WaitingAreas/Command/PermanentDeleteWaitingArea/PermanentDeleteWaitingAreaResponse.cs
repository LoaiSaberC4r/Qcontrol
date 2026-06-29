namespace Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;

public sealed record PermanentDeleteWaitingAreaResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
