using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Query.GetBranchServiceTree;

public sealed record GetBranchServiceTreeQuery
    : IQuery<IReadOnlyList<ServiceTreeNodeResponse>>
{
    public int BranchId { get; init; }

    public bool IncludeInactive { get; init; }

    public bool IncludeDeleted { get; init; }

    public string? SearchText { get; init; }
}
