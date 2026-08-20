using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.CreateBranchDisplayMessage;

internal sealed class CreateBranchDisplayMessageCommandValidator
    : AbstractValidator<CreateBranchDisplayMessageCommand>
{
    public CreateBranchDisplayMessageCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        RuleFor(x => x.TextAr).NotEmpty().WithMessage(BranchDisplayFeatureMessages.MessageTextRequired)
            .MaximumLength(500).WithMessage(BranchDisplayFeatureMessages.MessageTextMaxLength);
        RuleFor(x => x.TextEn).NotEmpty().WithMessage(BranchDisplayFeatureMessages.MessageTextRequired)
            .MaximumLength(500).WithMessage(BranchDisplayFeatureMessages.MessageTextMaxLength);
        RuleFor(x => x.DisplayOrder).GreaterThan(0).WithMessage(BranchDisplayFeatureMessages.InvalidDisplayOrder);
    }
}
