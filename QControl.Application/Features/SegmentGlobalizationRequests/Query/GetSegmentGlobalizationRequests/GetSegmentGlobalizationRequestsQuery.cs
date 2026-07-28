using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using FluentValidation;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequests;

public sealed class GetSegmentGlobalizationRequestsQuery
    : IQuery<Pagination<SegmentGlobalizationRequestResponse>>
{
    public SegmentGlobalizationRequestStatus? Status { get; set; }
    public int? BranchId { get; set; }
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderSort OrderSort { get; set; } = OrderSort.Newest;
}

internal sealed class GetSegmentGlobalizationRequestsQueryValidator
    : AbstractValidator<GetSegmentGlobalizationRequestsQuery>
{
    public GetSegmentGlobalizationRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(SegmentGlobalizationRequestMessages.PageNumberInvalid);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(SegmentGlobalizationRequestMessages.PageSizeInvalid);
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);
        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
    }
}
