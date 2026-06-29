using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreas;

public sealed class GetWaitingAreasQuery
    : IQuery<Pagination<WaitingAreaListItemResponse>>
{
    public int? BranchId { get; set; }

    public bool? IsActive { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
