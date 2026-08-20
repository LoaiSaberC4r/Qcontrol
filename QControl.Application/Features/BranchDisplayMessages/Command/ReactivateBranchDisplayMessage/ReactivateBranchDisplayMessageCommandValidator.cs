using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.ReactivateBranchDisplayMessage;

internal sealed class ReactivateBranchDisplayMessageCommandValidator
    : AbstractValidator<ReactivateBranchDisplayMessageCommand>
{
    public ReactivateBranchDisplayMessageCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.MessageId).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.MessageNotFound);
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
