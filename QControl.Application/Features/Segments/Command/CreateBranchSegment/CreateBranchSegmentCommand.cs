using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Segments.Command.CreateBranchSegment;

public sealed record CreateBranchSegmentCommand
    : ICommand<CreateBranchSegmentResponse>,
      ICacheInvalidator,
      ISegmentDefinitionRequest
{
    public int BranchId { get; init; }
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Segments,
        OperationalCacheTags.BranchServiceSegments,
        OperationalCacheTags.SegmentGlobalizationRequests,
        OperationalCacheTags.SegmentGlobalizationRequestsForBranch(BranchId)
    };
}

internal sealed class CreateBranchSegmentCommandValidator
    : AbstractValidator<CreateBranchSegmentCommand>
{
    public CreateBranchSegmentCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        SegmentValidationRules.Apply(this);
    }
}
