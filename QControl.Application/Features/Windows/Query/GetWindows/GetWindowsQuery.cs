using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.Windows.Query.GetWindows;

public sealed class GetWindowsQuery
    : IQuery<Pagination<WindowListItemResponse>>
{
    public int? WaitingAreaId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
