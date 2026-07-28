using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Segments.Command.CreateGlobalSegment;

public sealed record CreateGlobalSegmentCommand
    : ICommand<SegmentResponse>,
      ICacheInvalidator,
      ISegmentDefinitionRequest
{
    public string ArabicName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Segments,
        OperationalCacheTags.BranchServiceSegments
    };
}

internal sealed class CreateGlobalSegmentCommandValidator
    : AbstractValidator<CreateGlobalSegmentCommand>
{
    public CreateGlobalSegmentCommandValidator()
    {
        SegmentValidationRules.Apply(this);
    }
}

internal static class SegmentValidationRules
{
    public static void Apply<T>(AbstractValidator<T> validator)
        where T : class, ISegmentDefinitionRequest
    {
        validator.RuleFor(x => x.ArabicName)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(SegmentFeatureMessages.ArabicNameRequired)
            .MaximumLength(100)
            .WithMessage(SegmentFeatureMessages.NameMaxLength);
        validator.RuleFor(x => x.EnglishName)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage(SegmentFeatureMessages.EnglishNameRequired)
            .MaximumLength(100)
            .WithMessage(SegmentFeatureMessages.NameMaxLength);
        validator.RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .WithMessage(SegmentFeatureMessages.PriorityNonNegative);
    }
}

public interface ISegmentDefinitionRequest
{
    string ArabicName { get; }
    string EnglishName { get; }
    int Priority { get; }
}
