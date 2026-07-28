using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Segments.Command.UpdateSegment;

public sealed record UpdateSegmentCommand
    : ICommand<SegmentResponse>,
      ICacheInvalidator,
      ISegmentDefinitionRequest
{
    public int SegmentId { get; init; }
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Segments,
        OperationalCacheTags.Segment(SegmentId),
        OperationalCacheTags.BranchServiceSegments
    };
}

internal sealed class UpdateSegmentCommandValidator
    : AbstractValidator<UpdateSegmentCommand>
{
    public UpdateSegmentCommandValidator()
    {
        RuleFor(x => x.SegmentId).GreaterThan(0);
        SegmentValidationRules.Apply(this);
        RuleFor(x => x.RowVersion)
            .Must(x => RowVersionConverter.TryDecode(x, out _))
            .WithMessage(SegmentFeatureMessages.InvalidRowVersion);
    }
}
