using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayConfiguration;

internal sealed class GetBranchDisplayConfigurationQueryValidator
    : AbstractValidator<GetBranchDisplayConfigurationQuery>
{
    public GetBranchDisplayConfigurationQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
