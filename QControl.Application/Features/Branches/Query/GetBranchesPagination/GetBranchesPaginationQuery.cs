using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;

public sealed class GetBranchesPaginationQuery
    : SearchParameters,
      IQuery<Pagination<BranchPaginationItemResponse>>
{
    public bool? IsActive { get; set; }
}