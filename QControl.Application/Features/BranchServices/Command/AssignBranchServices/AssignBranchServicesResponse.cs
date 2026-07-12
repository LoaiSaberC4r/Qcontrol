namespace Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;

public sealed class AssignBranchServicesResponse
{
    public int BranchId { get; init; }

    public IReadOnlyList<int> RequestedLeafServiceIds { get; init; } =
        Array.Empty<int>();

    public IReadOnlyList<int> AssignedServiceIds { get; init; } =
        Array.Empty<int>();

    public int CreatedAssignmentsCount { get; init; }

    public int ExistingAssignmentsCount { get; init; }

    public string Message { get; init; } = string.Empty;
}
