using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.Services.Query.GetCentralServicesTree;

public sealed class GetCentralServicesTreeQuery
    : IQuery<IReadOnlyList<ServiceTreeNodeResponse>>
{
    public bool IncludeInactive { get; init; }

    public bool IncludeDeleted { get; init; }
}
