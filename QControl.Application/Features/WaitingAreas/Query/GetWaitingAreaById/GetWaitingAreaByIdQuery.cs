using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.WaitingAreas.Query.GetWaitingAreaById;

public sealed record GetWaitingAreaByIdQuery
    : IQuery<WaitingAreaDetailsResponse>
{
    public int Id { get; init; }
}
