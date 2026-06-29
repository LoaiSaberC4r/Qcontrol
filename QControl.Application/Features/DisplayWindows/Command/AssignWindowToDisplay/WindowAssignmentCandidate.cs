namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed record WindowAssignmentCandidate
{
    public int Id { get; init; }

    public int BranchId { get; init; }
}
