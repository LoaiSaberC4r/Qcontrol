namespace Qcontrol.Api.Contracts.BranchServices;

public sealed class AssignBranchServicesRequest
{
    public IReadOnlyCollection<int> ServiceIds { get; init; } =
        Array.Empty<int>();
}
