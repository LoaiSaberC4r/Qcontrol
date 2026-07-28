using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using FluentValidation;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Segments.Query.GetSegments;

public sealed class GetSegmentsQuery
    : IQuery<Pagination<SegmentResponse>>
{
    public string? SearchText { get; set; }
    public SegmentScope? Scope { get; set; }
    public int? OwnerBranchId { get; set; }
    public int? Priority { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderSort OrderSort { get; set; } = OrderSort.Newest;
}

internal sealed class GetSegmentsQueryValidator
    : AbstractValidator<GetSegmentsQuery>
{
    public GetSegmentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(SegmentFeatureMessages.PageNumberInvalid);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(SegmentFeatureMessages.PageSizeInvalid);
        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
        RuleFor(x => x.OwnerBranchId)
            .GreaterThan(0)
            .When(x => x.OwnerBranchId.HasValue);
        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Priority.HasValue);
    }
}
