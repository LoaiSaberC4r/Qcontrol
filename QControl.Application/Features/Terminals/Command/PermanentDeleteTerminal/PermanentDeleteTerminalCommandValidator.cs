using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

internal sealed class PermanentDeleteTerminalCommandValidator
    : AbstractValidator<PermanentDeleteTerminalCommand>
{
    public PermanentDeleteTerminalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);
    }
}
