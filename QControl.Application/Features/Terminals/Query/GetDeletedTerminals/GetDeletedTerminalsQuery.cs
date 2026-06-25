using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace Qcontrol.Application.Features.Terminals.Query.GetDeletedTerminals;

public sealed class GetDeletedTerminalsQuery
    : IQuery<Pagination<DeletedTerminalListItemResponse>>
{
    public int? WindowId { get; set; }

    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
