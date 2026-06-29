namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed record DisplayAssignmentCandidate
{
    public int Id { get; init; }

    public int BranchId { get; init; }
}
