using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;

internal sealed class GetBranchConfigurationQueryValidator
    : AbstractValidator<GetBranchConfigurationQuery>
{
    public GetBranchConfigurationQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required)
            .WithErrorCode("BranchConfigurations.View.BranchIdInvalid");
    }
}
