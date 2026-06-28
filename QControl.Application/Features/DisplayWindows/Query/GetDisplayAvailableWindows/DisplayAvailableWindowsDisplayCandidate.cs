namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

internal sealed record DisplayAvailableWindowsDisplayCandidate
{
    public int Id { get; init; }

    public int BranchId { get; init; }

    public bool IsDeleted { get; init; }
}
