using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServiceSegments.Command.AssignBranchServiceSegments;

public sealed class AssignBranchServiceSegmentItem
{
    public int SegmentId { get; init; }
    public int Quota { get; init; }
}

public sealed record AssignBranchServiceSegmentsCommand
    : ICommand<AssignedBranchServiceSegmentsResponse>, ICacheInvalidator
{
    public int BranchId { get; init; }
    public int LeafServiceId { get; init; }
    public IReadOnlyList<AssignBranchServiceSegmentItem> Items { get; init; } =
        Array.Empty<AssignBranchServiceSegmentItem>();
    public int? BranchServiceIdForInvalidation { get; set; }

    public IEnumerable<string> Tags
    {
        get
        {
            yield return OperationalCacheTags.BranchServiceSegments;
            yield return
                OperationalCacheTags.BranchServiceSegmentsForBranchAndService(
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

internal sealed class AssignBranchServiceSegmentsCommandValidator
    : AbstractValidator<AssignBranchServiceSegmentsCommand>
{
    public AssignBranchServiceSegmentsCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.LeafServiceId).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(BranchServiceSegmentMessages.ItemsRequired);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.SegmentId).GreaterThan(0);
            item.RuleFor(x => x.Quota)
                .GreaterThanOrEqualTo(0)
                .WithMessage(BranchServiceSegmentMessages.QuotaNonNegative);
        });
        RuleFor(x => x.Items)
            .Must(items =>
                items.Select(x => x.SegmentId).Distinct().Count() ==
                items.Count)
            .WithMessage(BranchServiceSegmentMessages.DuplicateSegmentIds);
    }
}
