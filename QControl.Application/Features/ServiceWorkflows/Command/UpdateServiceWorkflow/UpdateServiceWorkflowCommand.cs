using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceWorkflows.Command.UpdateServiceWorkflow;

public sealed record UpdateServiceWorkflowCommand
    : ICommand<ServiceWorkflowResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public IReadOnlyList<ServiceWorkflowStepCommandItem> Steps { get; init; } =
        Array.Empty<ServiceWorkflowStepCommandItem>();

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.ServiceWorkflows,
        OperationalCacheTags.ServiceWorkflow(Id),
        OperationalCacheTags.Services
    };
}
