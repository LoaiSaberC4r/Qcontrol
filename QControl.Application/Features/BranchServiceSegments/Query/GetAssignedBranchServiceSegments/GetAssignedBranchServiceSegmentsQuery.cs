using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;

namespace Qcontrol.Application.Features.BranchServiceSegments.Query.GetAssignedBranchServiceSegments;

public sealed class GetAssignedBranchServiceSegmentsQuery
    : IQuery<AssignedBranchServiceSegmentsResponse>
{
    public int BranchId { get; set; }
    public int LeafServiceId { get; set; }
}

internal sealed class GetAssignedBranchServiceSegmentsQueryValidator
    : AbstractValidator<GetAssignedBranchServiceSegmentsQuery>
{
    public GetAssignedBranchServiceSegmentsQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.LeafServiceId).GreaterThan(0);
    }
}
