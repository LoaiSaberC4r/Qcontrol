using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

internal sealed class PermanentDeleteDisplayCommandValidator
    : AbstractValidator<PermanentDeleteDisplayCommand>
{
    public PermanentDeleteDisplayCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);
    }
}
