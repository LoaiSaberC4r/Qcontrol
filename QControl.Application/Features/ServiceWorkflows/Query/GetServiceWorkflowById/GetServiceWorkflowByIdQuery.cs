using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceWorkflows.Shared;

namespace Qcontrol.Application.Features.ServiceWorkflows.Query.GetServiceWorkflowById;

public sealed class GetServiceWorkflowByIdQuery
    : IQuery<ServiceWorkflowResponse>
{
    public int Id { get; init; }
}
