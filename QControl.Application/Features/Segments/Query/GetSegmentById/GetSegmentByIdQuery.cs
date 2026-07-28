using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.Segments.Shared;

namespace Qcontrol.Application.Features.Segments.Query.GetSegmentById;

public sealed class GetSegmentByIdQuery : IQuery<SegmentDetailsResponse>
{
    public int SegmentId { get; set; }
}

internal sealed class GetSegmentByIdQueryValidator
    : AbstractValidator<GetSegmentByIdQuery>
{
    public GetSegmentByIdQueryValidator()
    {
        RuleFor(x => x.SegmentId).GreaterThan(0);
    }
}
