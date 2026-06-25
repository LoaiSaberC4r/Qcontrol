using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Command.RestoreTerminal;

internal sealed class RestoreTerminalCommandValidator
    : AbstractValidator<RestoreTerminalCommand>
{
    public RestoreTerminalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);
    }
}
