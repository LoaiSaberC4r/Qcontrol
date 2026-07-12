using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;

internal sealed class CreateBranchServiceSubtreeCommandValidator
    : AbstractValidator<CreateBranchServiceSubtreeCommand>
{
    public CreateBranchServiceSubtreeCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);

        RuleFor(x => x.ParentServiceId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.ParentIdRequired);

        RuleFor(x => x.Root)
            .NotNull()
            .WithMessage(ServiceFeatureMessages.BranchServiceSubtreeRootRequired);
    }
}
