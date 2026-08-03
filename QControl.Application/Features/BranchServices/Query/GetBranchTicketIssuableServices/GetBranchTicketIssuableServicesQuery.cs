using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.BranchServices.Query
    .GetBranchTicketIssuableServices;

public sealed record GetBranchTicketIssuableServicesQuery
    : IQuery<GetBranchTicketIssuableServicesResponse>
{
    public int BranchId { get; init; }
}
