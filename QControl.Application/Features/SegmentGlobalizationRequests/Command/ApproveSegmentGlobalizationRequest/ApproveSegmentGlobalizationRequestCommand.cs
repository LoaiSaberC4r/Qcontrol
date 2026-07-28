using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.ApproveSegmentGlobalizationRequest;

public sealed record ApproveSegmentGlobalizationRequestCommand
    : ICommand<SegmentGlobalizationRequestResponse>, ICacheInvalidator
{
    public int RequestId { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public int? BranchIdForInvalidation { get; set; }
    public int? SegmentIdForInvalidation { get; set; }
    public IEnumerable<string> Tags
    {
        get
        {
            yield return OperationalCacheTags.Segments;
            yield return OperationalCacheTags.BranchServiceSegments;
            yield return OperationalCacheTags.SegmentGlobalizationRequests;
            yield return OperationalCacheTags
                .SegmentGlobalizationRequest(RequestId);
            if (BranchIdForInvalidation.HasValue)
            {
                yield return OperationalCacheTags
                    .SegmentGlobalizationRequestsForBranch(
                        BranchIdForInvalidation.Value);
            }
            if (SegmentIdForInvalidation.HasValue)
            {
                yield return OperationalCacheTags.Segment(
                    SegmentIdForInvalidation.Value);
            }
        }
    }
}

internal sealed class ApproveSegmentGlobalizationRequestCommandValidator
    : AbstractValidator<ApproveSegmentGlobalizationRequestCommand>
{
    public ApproveSegmentGlobalizationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.RowVersion)
            .Must(x => RowVersionConverter.TryDecode(x, out _))
            .WithMessage(SegmentGlobalizationRequestMessages.InvalidRowVersion);
    }
}
