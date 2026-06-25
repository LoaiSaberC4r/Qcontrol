using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.Windows.Query.GetDeletedWindows;

public sealed class GetDeletedWindowsQuery
    : IQuery<Pagination<DeletedWindowListItemResponse>>
{
    public int? WaitingAreaId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
