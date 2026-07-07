using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetServiceById;

public sealed record GetServiceByIdQuery
    : IQuery<ServiceDetailsResponse>
{
    public int Id { get; init; }
}
