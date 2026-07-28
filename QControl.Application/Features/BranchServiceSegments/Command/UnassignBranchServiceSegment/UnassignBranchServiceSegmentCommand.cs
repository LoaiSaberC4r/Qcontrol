using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServiceSegments.Command.UnassignBranchServiceSegment;

public sealed record UnassignBranchServiceSegmentCommand
    : ICommand<UnassignBranchServiceSegmentResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public int LeafServiceId { get; init; }
    public int SegmentId { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public int? BranchServiceIdForInvalidation { get; set; }
    public IEnumerable<string> Tags
    {
        get
        {
            yield return OperationalCacheTags.BranchServiceSegments;
            yield return OperationalCacheTags
                .BranchServiceSegmentsForBranchAndService(
                    BranchId,
                    LeafServiceId);
            if (BranchServiceIdForInvalidation.HasValue)
            {
                yield return OperationalCacheTags
                    .BranchServiceSegmentsForBranchService(
                        BranchServiceIdForInvalidation.Value);
            }
        }
    }
}

internal sealed class UnassignBranchServiceSegmentCommandValidator
    : AbstractValidator<UnassignBranchServiceSegmentCommand>
{
    public UnassignBranchServiceSegmentCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.LeafServiceId).GreaterThan(0);
        RuleFor(x => x.SegmentId).GreaterThan(0);
        RuleFor(x => x.RowVersion)
            .Must(x => RowVersionConverter.TryDecode(x, out _))
            .WithMessage(BranchServiceSegmentMessages.InvalidRowVersion);
    }
}
