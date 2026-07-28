using BuildingBlock.Application.Abstraction;
using FluentValidation;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.RejectSegmentGlobalizationRequest;

public sealed record RejectSegmentGlobalizationRequestCommand
    : ICommand<SegmentGlobalizationRequestResponse>, ICacheInvalidator
{
    public int RequestId { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public int? BranchIdForInvalidation { get; set; }
    public IEnumerable<string> Tags
    {
        get
        {
            yield return OperationalCacheTags.SegmentGlobalizationRequests;
            yield return OperationalCacheTags
                .SegmentGlobalizationRequest(RequestId);
            if (BranchIdForInvalidation.HasValue)
            {
                yield return OperationalCacheTags
                    .SegmentGlobalizationRequestsForBranch(
                        BranchIdForInvalidation.Value);
            }
        }
    }
}

internal sealed class RejectSegmentGlobalizationRequestCommandValidator
    : AbstractValidator<RejectSegmentGlobalizationRequestCommand>
{
    public RejectSegmentGlobalizationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.RowVersion)
            .Must(x => RowVersionConverter.TryDecode(x, out _))
            .WithMessage(SegmentGlobalizationRequestMessages.InvalidRowVersion);
        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .WithMessage(
                SegmentGlobalizationRequestMessages.RejectionReasonMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.RejectionReason));
    }
}
