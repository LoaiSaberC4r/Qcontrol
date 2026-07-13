using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServices.Command.UnassignBranchLeafService;

internal sealed class UnassignBranchLeafServiceCommandValidator
    : AbstractValidator<UnassignBranchLeafServiceCommand>
{
    public UnassignBranchLeafServiceCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);

        RuleFor(x => x.LeafServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.IdRequired);
    }
}