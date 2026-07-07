using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetAvailableParentServices;

public sealed record GetAvailableParentServicesQuery
    : IQuery<IReadOnlyList<AvailableParentServiceResponse>>
{
    public int? ExcludeServiceId { get; init; }

    public string? SearchText { get; init; }
}
