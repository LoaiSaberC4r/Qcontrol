using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Windows.Query.GetWindowById;

public sealed record GetWindowByIdQuery
    : IQuery<WindowDetailsResponse>
{
    public int Id { get; init; }
}
