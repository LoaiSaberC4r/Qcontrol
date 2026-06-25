using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminalById;

public sealed record GetTerminalByIdQuery
    : IQuery<TerminalDetailsResponse>
{
    public int Id { get; init; }
}
