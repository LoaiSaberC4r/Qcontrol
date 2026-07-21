using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Query.GetServicesTree;

public sealed record GetServicesTreeQuery
    : IQuery<IReadOnlyList<ServiceTreeNodeResponse>>
{
    public bool IncludeInactive { get; init; }

    public bool IncludeDeleted { get; init; }

    public ServiceScope? Scope { get; init; }

    public int? OwnerBranchId { get; init; }

    public string? SearchText { get; init; }
}
