using FluentValidation;
using Qcontrol.Domain.Resources;

namespace Qcontrol.Application.Features.Displays.Command.RestoreDisplay;

internal sealed class RestoreDisplayCommandValidator
    : AbstractValidator<RestoreDisplayCommand>
{
    public RestoreDisplayCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(ErrorMessage.Display_Id_Required);
    }
}
