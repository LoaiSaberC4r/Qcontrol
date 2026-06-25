using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;

public sealed class GetDeletedDisplaysQuery
    : IQuery<Pagination<DeletedDisplayListItemResponse>>
{
    public int? BranchId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
