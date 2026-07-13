using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequestById;

public sealed class GetServiceGlobalizationRequestByIdQuery
    : IQuery<ServiceGlobalizationRequestDetailsResponse>
{
    public int RequestId { get; set; }
}
