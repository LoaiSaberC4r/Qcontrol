namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

public sealed record PermanentDeleteDisplayResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
