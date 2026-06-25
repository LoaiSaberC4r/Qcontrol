namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

internal sealed record PermanentDeleteDisplayCandidate
{
    public int Id { get; init; }

    public bool IsDeleted { get; init; }
}
