using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

public sealed class GetDisplayAvailableWindowsQuery
    : IQuery<Pagination<AvailableWindowResponse>>
{
    public int DisplayId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
