using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;

internal sealed class UpdateBranchDisplayConfigurationCommandValidator
    : AbstractValidator<UpdateBranchDisplayConfigurationCommand>
{
    public UpdateBranchDisplayConfigurationCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        this.AddDisplayConfigurationRules();
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
