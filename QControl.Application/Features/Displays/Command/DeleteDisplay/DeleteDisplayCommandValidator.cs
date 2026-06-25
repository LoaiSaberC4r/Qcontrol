using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Command.DeleteDisplay;

internal sealed class DeleteDisplayCommandValidator
    : AbstractValidator<DeleteDisplayCommand>
{
    public DeleteDisplayCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);
    }
}
