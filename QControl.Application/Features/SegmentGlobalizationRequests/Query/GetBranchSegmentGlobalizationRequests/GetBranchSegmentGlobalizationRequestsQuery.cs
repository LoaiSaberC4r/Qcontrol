using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.SharedDto;
using FluentValidation;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetBranchSegmentGlobalizationRequests;

public sealed class GetBranchSegmentGlobalizationRequestsQuery
    : IQuery<Pagination<SegmentGlobalizationRequestResponse>>
{
    public int BranchId { get; set; }
    public SegmentGlobalizationRequestStatus? Status { get; set; }
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderSort OrderSort { get; set; } = OrderSort.Newest;
}

internal sealed class GetBranchSegmentGlobalizationRequestsQueryValidator
    : AbstractValidator<GetBranchSegmentGlobalizationRequestsQuery>
{
    public GetBranchSegmentGlobalizationRequestsQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SearchText)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchText));
    }
}
