using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequestById;

public sealed class GetSegmentGlobalizationRequestByIdQuery
    : IQuery<SegmentGlobalizationRequestResponse>
{
    public int RequestId { get; set; }
}

internal sealed class GetSegmentGlobalizationRequestByIdQueryValidator
    : AbstractValidator<GetSegmentGlobalizationRequestByIdQuery>
{
    public GetSegmentGlobalizationRequestByIdQueryValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
    }
}
