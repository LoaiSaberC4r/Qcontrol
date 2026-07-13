namespace Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;

public sealed record UnassignBranchLeafServiceResponse
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public IReadOnlyList<int> UnassignedServiceIds { get; init; } =
        Array.Empty<int>();

    public int RemovedAssignmentsCount { get; init; }

    public string Message { get; init; } = string.Empty;
}