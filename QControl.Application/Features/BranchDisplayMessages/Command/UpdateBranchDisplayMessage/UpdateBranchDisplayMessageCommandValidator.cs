using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.UpdateBranchDisplayMessage;

internal sealed class UpdateBranchDisplayMessageCommandValidator
    : AbstractValidator<UpdateBranchDisplayMessageCommand>
{
    public UpdateBranchDisplayMessageCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.MessageId).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.MessageNotFound);
        RuleFor(x => x.TextAr).NotEmpty().WithMessage(BranchDisplayFeatureMessages.MessageTextRequired)
            .MaximumLength(500).WithMessage(BranchDisplayFeatureMessages.MessageTextMaxLength);
        RuleFor(x => x.TextEn).NotEmpty().WithMessage(BranchDisplayFeatureMessages.MessageTextRequired)
            .MaximumLength(500).WithMessage(BranchDisplayFeatureMessages.MessageTextMaxLength);
        RuleFor(x => x.DisplayOrder).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.InvalidDisplayOrder);
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
