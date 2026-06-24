using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

public sealed record GetBranchDetailsQuery
    : IQuery<GetBranchDetailsResponse>
{
    public int BranchId { get; init; }
}