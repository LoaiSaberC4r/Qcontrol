using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Branches.Command.DeleteBranch;

internal sealed class DeleteBranchCommandValidator
    : AbstractValidator<DeleteBranchCommand>
{
    public DeleteBranchCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);
    }
}