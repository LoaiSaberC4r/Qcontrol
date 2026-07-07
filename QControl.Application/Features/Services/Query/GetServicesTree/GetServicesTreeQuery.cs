using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetServicesTree;

public sealed record GetServicesTreeQuery
    : IQuery<IReadOnlyList<ServiceTreeNodeResponse>>
{
    public bool IncludeInactive { get; init; }

    public bool IncludeDeleted { get; init; }
}
