using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.DeactivateServiceWorkflow;

public sealed record DeactivateServiceWorkflowCommand
    : ICommand<ServiceWorkflowActivationResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.ServiceWorkflows,
        OperationalCacheTags.ServiceWorkflow(Id),
        OperationalCacheTags.BranchServiceWorkflows(BranchId, LeafServiceId),
        OperationalCacheTags.BranchWorkflowCandidates(BranchId),
        OperationalCacheTags.Services
    };
}
