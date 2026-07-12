using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Command.AssignBranchServices;

internal sealed class AssignBranchServicesCommandValidator
    : AbstractValidator<AssignBranchServicesCommand>
{
    public AssignBranchServicesCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);

        RuleFor(x => x.ServiceIds)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage(ServiceFeatureMessages.LeafIdsRequired);

        RuleForEach(x => x.ServiceIds)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);
    }
}
