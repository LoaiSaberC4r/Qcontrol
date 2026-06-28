namespace Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;

public sealed record UnassignWindowFromDisplayResponse
{
    public int DisplayId { get; init; }

    public int WindowId { get; init; }

    public string Message { get; init; } = string.Empty;
}
