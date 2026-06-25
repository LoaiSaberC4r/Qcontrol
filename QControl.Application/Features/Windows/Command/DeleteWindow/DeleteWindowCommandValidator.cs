using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Windows.Command.DeleteWindow;

internal sealed class DeleteWindowCommandValidator
    : AbstractValidator<DeleteWindowCommand>
{
    public DeleteWindowCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Window_Id_Required);
    }
}
