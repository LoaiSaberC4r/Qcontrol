using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchBranding.Shared;

namespace Qcontrol.Application.Features.BranchBranding.Query.GetBranchBranding;

public sealed record GetBranchBrandingQuery
    : IQuery<BranchBrandingResponse>
{
    public int BranchId { get; init; }
}
