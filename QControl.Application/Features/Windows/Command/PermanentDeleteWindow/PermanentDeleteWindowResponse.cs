namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

public sealed record PermanentDeleteWindowResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
