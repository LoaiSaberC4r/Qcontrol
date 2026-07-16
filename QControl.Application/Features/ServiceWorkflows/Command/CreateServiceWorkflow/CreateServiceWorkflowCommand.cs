using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.CreateServiceWorkflow;

public sealed record CreateServiceWorkflowCommand
    : ICommand<ServiceWorkflowResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int LeafServiceId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public IReadOnlyList<ServiceWorkflowStepCommandItem> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepCommandItem>();

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.ServiceWorkflows,
        OperationalCacheTags.BranchServiceWorkflows(BranchId, LeafServiceId),
        OperationalCacheTags.BranchWorkflowCandidates(BranchId),
        OperationalCacheTags.BranchServices,
        OperationalCacheTags.BranchServicesForBranch(BranchId),
        OperationalCacheTags.Services
    };
}
