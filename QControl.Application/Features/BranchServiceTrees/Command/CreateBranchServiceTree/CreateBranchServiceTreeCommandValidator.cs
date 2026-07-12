using FluentValidation;
using Qcontrol.Application.Features.Services.Shared;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;

internal sealed class CreateBranchServiceTreeCommandValidator
    : AbstractValidator<CreateBranchServiceTreeCommand>
{
    public CreateBranchServiceTreeCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ServiceFeatureMessages.BranchNotFound);

        RuleFor(x => x.Root)
            .NotNull()
            .WithMessage(ServiceFeatureMessages.BranchServiceTreeRootRequired);
    }
}
