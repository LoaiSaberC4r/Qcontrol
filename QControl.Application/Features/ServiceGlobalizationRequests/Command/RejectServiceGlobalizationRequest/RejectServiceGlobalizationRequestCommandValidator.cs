using FluentValidation;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.RejectServiceGlobalizationRequest;

internal sealed class RejectServiceGlobalizationRequestCommandValidator
    : AbstractValidator<RejectServiceGlobalizationRequestCommand>
{
    public RejectServiceGlobalizationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage(ServiceGlobalizationRequestMessages.RequestIdRequired);

        RuleFor(x => x.RowVersion)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ServiceGlobalizationRequestMessages.InvalidRowVersion);

        RuleFor(x => x.RejectionReason)
            .MaximumLength(1000)
            .WithMessage(
                ServiceGlobalizationRequestMessages.RejectionReasonMaxLength)
            .When(x => x.RejectionReason is not null);
    }
}
