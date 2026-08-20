using FluentValidation;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.CreateBranchDisplayConfiguration;

internal sealed class CreateBranchDisplayConfigurationCommandValidator
    : AbstractValidator<CreateBranchDisplayConfigurationCommand>
{
    public CreateBranchDisplayConfigurationCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
        this.AddDisplayConfigurationRules();
    }
}
