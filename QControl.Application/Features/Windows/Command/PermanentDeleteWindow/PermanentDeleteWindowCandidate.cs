namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

internal sealed record PermanentDeleteWindowCandidate
{
    public int Id { get; init; }

    public bool IsDeleted { get; init; }
}
