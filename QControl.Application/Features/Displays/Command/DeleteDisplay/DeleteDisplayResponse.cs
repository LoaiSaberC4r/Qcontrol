namespace Qcontrol.Application.Features.Displays.Command.DeleteDisplay;

public sealed record DeleteDisplayResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
