using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

internal sealed class GetBranchDisplayRuntimeConfigurationQueryValidator
    : AbstractValidator<GetBranchDisplayRuntimeConfigurationQuery>
{
    public GetBranchDisplayRuntimeConfigurationQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage(ErrorMessage.Branch_Id_Required);
    }
}
