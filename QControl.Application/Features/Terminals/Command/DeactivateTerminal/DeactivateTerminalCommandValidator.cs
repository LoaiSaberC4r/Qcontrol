using FluentValidation;
using Qcontrol.Domain.Resources;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;

internal sealed class DeactivateTerminalCommandValidator
    : AbstractValidator<DeactivateTerminalCommand>
{
    public DeactivateTerminalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage(ErrorMessage.RowVersion_Required)
            .Must(value => RowVersionConverter.TryDecode(value, out _))
            .WithMessage(ErrorMessage.RowVersion_Invalid);
    }
}
