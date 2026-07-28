using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceSegments.Query.GetAvailableBranchServiceSegments;

public sealed class GetAvailableBranchServiceSegmentsQuery
    : IQuery<AvailableBranchServiceSegmentsResponse>
{
    public int BranchId { get; set; }
    public int LeafServiceId { get; set; }
    public string? SearchText { get; set; }
    public SegmentScope? Scope { get; set; }
    public int? Priority { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

internal sealed class GetAvailableBranchServiceSegmentsQueryValidator
    : AbstractValidator<GetAvailableBranchServiceSegmentsQuery>
{
    public GetAvailableBranchServiceSegmentsQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.LeafServiceId).GreaterThan(0);
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(BranchServiceSegmentMessages.PageNumberInvalid);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(BranchServiceSegmentMessages.PageSizeInvalid);
        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Priority.HasValue);
    }
}
