using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Terminals.Command.DeleteTerminal;

internal sealed class DeleteTerminalCommandValidator
    : AbstractValidator<DeleteTerminalCommand>
{
    public DeleteTerminalCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Terminal_Id_Required);
    }
}
