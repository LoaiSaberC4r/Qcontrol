namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

public sealed record AssignWindowToDisplayResponse
{
    public int DisplayId { get; init; }

    public int WindowId { get; init; }

    public string Message { get; init; } = string.Empty;
}
