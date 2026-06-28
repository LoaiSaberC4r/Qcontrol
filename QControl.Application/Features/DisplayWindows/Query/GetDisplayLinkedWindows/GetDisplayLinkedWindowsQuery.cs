using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

public sealed class GetDisplayLinkedWindowsQuery
    : IQuery<Pagination<DisplayLinkedWindowResponse>>
{
    public int DisplayId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
