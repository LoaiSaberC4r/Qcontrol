using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowsForService;

public sealed class GetServiceWorkflowsForServiceQuery
    : IQuery<ServiceWorkflowForServiceResponse>
{
    public int ServiceId { get; init; }
}
