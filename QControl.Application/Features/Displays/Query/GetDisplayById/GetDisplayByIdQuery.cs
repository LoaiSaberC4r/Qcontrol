using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplayById;

public sealed record GetDisplayByIdQuery
    : IQuery<DisplayDetailsResponse>
{
    public int Id { get; init; }
}
