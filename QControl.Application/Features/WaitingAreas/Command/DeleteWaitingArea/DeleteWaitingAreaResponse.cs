namespace Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;

public sealed record DeleteWaitingAreaResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
