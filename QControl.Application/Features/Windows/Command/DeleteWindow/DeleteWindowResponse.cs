namespace Qcontrol.Application.Features.Windows.Command.DeleteWindow;

public sealed record DeleteWindowResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
