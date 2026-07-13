using FluentValidation;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.ApproveServiceGlobalizationRequest;

internal sealed class ApproveServiceGlobalizationRequestCommandValidator
    : AbstractValidator<ApproveServiceGlobalizationRequestCommand>
{
    public ApproveServiceGlobalizationRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage(ServiceGlobalizationRequestMessages.RequestIdRequired);

        RuleFor(x => x.RowVersion)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ServiceGlobalizationRequestMessages.InvalidRowVersion);
    }
}
