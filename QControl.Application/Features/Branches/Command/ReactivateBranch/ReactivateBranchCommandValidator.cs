using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Branches.Command.ReactivateBranch;

internal sealed class ReactivateBranchCommandValidator
    : AbstractValidator<ReactivateBranchCommand>
{
    public ReactivateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Branch_Id_Required);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
