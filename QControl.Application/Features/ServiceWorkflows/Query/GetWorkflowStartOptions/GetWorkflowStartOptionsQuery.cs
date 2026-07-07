using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetWorkflowStartOptions;

public sealed class GetWorkflowStartOptionsQuery
    : IQuery<ServiceWorkflowStartOptionsResponse>
{
    public int ServiceId { get; init; }
}
